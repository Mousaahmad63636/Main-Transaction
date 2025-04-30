using QuickTechPOS.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace QuickTechPOS.Services
{
    /// <summary>
    /// Provides operations for managing cash drawers
    /// </summary>
    public class DrawerService
    {
        private readonly DatabaseContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the drawer service
        /// </summary>
        public DrawerService()
        {
            _dbContext = new DatabaseContext(ConfigurationService.ConnectionString);
        }

        /// <summary>
        /// Opens a new drawer session
        /// </summary>
        /// <param name="cashierId">ID of the cashier</param>
        /// <param name="cashierName">Name of the cashier</param>
        /// <param name="openingBalance">Initial cash amount in the drawer</param>
        /// <param name="notes">Additional notes for this drawer session</param>
        /// <returns>The created drawer record</returns>
        public async Task<Drawer> OpenDrawerAsync(string cashierId, string cashierName, decimal openingBalance, string notes = "")
        {
            try
            {
                // Check if there's already an open drawer for this cashier
                var existingOpenDrawer = await _dbContext.Drawers
                    .Where(d => d.CashierId == cashierId && d.Status == "Open")
                    .FirstOrDefaultAsync();

                if (existingOpenDrawer != null)
                {
                    // Return the existing open drawer instead of creating a new one
                    return existingOpenDrawer;
                }

                // Create a new drawer record
                var drawer = new Drawer
                {
                    OpeningBalance = openingBalance,
                    CurrentBalance = openingBalance,
                    CashIn = 0,
                    CashOut = 0,
                    TotalSales = 0,
                    TotalExpenses = 0,
                    TotalSupplierPayments = 0,
                    NetCashFlow = 0,
                    DailySales = 0,
                    DailyExpenses = 0,
                    DailySupplierPayments = 0,
                    OpenedAt = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    NetSales = 0,
                    Status = "Open",
                    CashierId = cashierId,
                    CashierName = cashierName,
                    Notes = notes ?? string.Empty // Ensure notes is never null
                };

                _dbContext.Drawers.Add(drawer);
                await _dbContext.SaveChangesAsync();

                return drawer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OpenDrawerAsync: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Performs a cash out operation on a drawer
        /// </summary>
        /// <param name="drawerId">ID of the drawer</param>
        /// <param name="cashOutAmount">Amount of cash to take out</param>
        /// <param name="notes">Reason for the cash out</param>
        /// <returns>The updated drawer record</returns>
        public async Task<Drawer> PerformCashOutAsync(int drawerId, decimal cashOutAmount, string notes)
        {
            try
            {
                var drawer = await _dbContext.Drawers.FindAsync(drawerId);
                if (drawer == null)
                {
                    throw new ArgumentException($"Drawer with ID {drawerId} not found.");
                }

                if (drawer.Status != "Open")
                {
                    throw new InvalidOperationException("Cannot perform cash out on a drawer that is not open.");
                }

                if (cashOutAmount <= 0)
                {
                    throw new ArgumentException("Cash out amount must be greater than zero.");
                }

                if (cashOutAmount > drawer.CurrentBalance)
                {
                    throw new ArgumentException("Cash out amount cannot exceed current drawer balance.");
                }

                // Update cash out amount
                drawer.CashOut += cashOutAmount;

                // Update current balance and net cash flow
                drawer.CurrentBalance -= cashOutAmount;
                drawer.NetCashFlow = drawer.CashIn - drawer.CashOut + drawer.TotalSales - drawer.TotalExpenses - drawer.TotalSupplierPayments;

                // Update timestamp
                drawer.LastUpdated = DateTime.Now;

                // Update notes - append cash out notes to existing notes
                string cashOutNote = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Cash Out: ${cashOutAmount:F2} - {notes}";
                if (string.IsNullOrEmpty(drawer.Notes))
                    drawer.Notes = cashOutNote;
                else
                    drawer.Notes = $"{drawer.Notes}\n{cashOutNote}";

                await _dbContext.SaveChangesAsync();

                return drawer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PerformCashOutAsync: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Closes an open drawer session
        /// </summary>
        /// <param name="drawerId">ID of the drawer to close</param>
        /// <param name="closingBalance">Final cash amount in the drawer</param>
        /// <param name="closingNotes">Notes about closing the drawer</param>
        /// <returns>The updated drawer record</returns>
        public async Task<Drawer> CloseDrawerAsync(int drawerId, decimal closingBalance, string closingNotes = "")
        {
            try
            {
                var drawer = await _dbContext.Drawers.FindAsync(drawerId);
                if (drawer == null)
                {
                    throw new ArgumentException($"Drawer with ID {drawerId} not found.");
                }

                if (drawer.Status != "Open")
                {
                    throw new InvalidOperationException("Cannot close a drawer that is not open.");
                }

                drawer.CurrentBalance = closingBalance;
                drawer.ClosedAt = DateTime.Now;
                drawer.LastUpdated = DateTime.Now;
                drawer.Status = "Closed";

                // Update notes - append closing notes to existing notes
                if (!string.IsNullOrEmpty(closingNotes))
                {
                    if (string.IsNullOrEmpty(drawer.Notes))
                        drawer.Notes = $"Closing notes: {closingNotes}";
                    else
                        drawer.Notes = $"{drawer.Notes}\nClosing notes: {closingNotes}";
                }

                await _dbContext.SaveChangesAsync();

                return drawer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CloseDrawerAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the currently open drawer for a cashier
        /// </summary>
        /// <param name="cashierId">ID of the cashier</param>
        /// <returns>The open drawer or null if none exists</returns>
        public async Task<Drawer> GetOpenDrawerAsync(string cashierId)
        {
            try
            {
                var drawer = await _dbContext.Drawers
                    .Where(d => d.CashierId == cashierId && d.Status == "Open")
                    .FirstOrDefaultAsync();

                // Ensure Notes is never null
                if (drawer != null && drawer.Notes == null)
                {
                    drawer.Notes = string.Empty;
                }

                return drawer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetOpenDrawerAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates sales and expenses data for a drawer
        /// </summary>
        /// <param name="drawerId">ID of the drawer to update</param>
        /// <param name="salesAmount">Amount of sales to add</param>
        /// <param name="expensesAmount">Amount of expenses to add</param>
        /// <param name="supplierPayments">Amount of supplier payments to add</param>
        /// <returns>The updated drawer record</returns>
        public async Task<Drawer> UpdateDrawerTransactionsAsync(int drawerId, decimal salesAmount = 0, decimal expensesAmount = 0, decimal supplierPayments = 0)
        {
            try
            {
                var drawer = await _dbContext.Drawers.FindAsync(drawerId);
                if (drawer == null)
                {
                    throw new ArgumentException($"Drawer with ID {drawerId} not found.");
                }

                if (drawer.Status != "Open")
                {
                    throw new InvalidOperationException("Cannot update a drawer that is not open.");
                }

                // Update daily and total sales
                drawer.DailySales += salesAmount;
                drawer.TotalSales += salesAmount;

                // Update daily and total expenses
                drawer.DailyExpenses += expensesAmount;
                drawer.TotalExpenses += expensesAmount;

                // Update daily and total supplier payments
                drawer.DailySupplierPayments += supplierPayments;
                drawer.TotalSupplierPayments += supplierPayments;

                // Update net cash flow and current balance
                decimal cashInflow = salesAmount;
                decimal cashOutflow = expensesAmount + supplierPayments;
                drawer.NetCashFlow = drawer.CashIn - drawer.CashOut + drawer.TotalSales - drawer.TotalExpenses - drawer.TotalSupplierPayments;
                drawer.CurrentBalance += (cashInflow - cashOutflow);
                drawer.NetSales = drawer.TotalSales - drawer.TotalExpenses;

                // Update timestamp
                drawer.LastUpdated = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return drawer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateDrawerTransactionsAsync: {ex.Message}");
                throw;
            }
        }
    }
}