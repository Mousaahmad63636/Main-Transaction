// File: QuickTechPOS/Services/DrawerService.cs (key updated methods)

using QuickTechPOS.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

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

                // Create a drawer transaction for the opening balance
                var drawerTransaction = new DrawerTransaction
                {
                    DrawerId = drawer.DrawerId,
                    Timestamp = DateTime.Now,
                    Type = "Open",
                    Amount = openingBalance,
                    Balance = openingBalance,
                    ActionType = "Open",
                    Description = "Drawer opened with initial balance",
                    TransactionReference = drawer.DrawerId.ToString(),
                    IsVoided = false,
                    PaymentMethod = "Cash"
                };

                _dbContext.DrawerTransactions.Add(drawerTransaction);
                await _dbContext.SaveChangesAsync();

                // Also create a drawer history entry
                var historyEntry = new DrawerHistoryEntry
                {
                    Timestamp = DateTime.Now,
                    ActionType = "Open",
                    Description = "Drawer opened with initial balance",
                    Amount = openingBalance,
                    ResultingBalance = openingBalance,
                    UserId = cashierId
                };

                _dbContext.DrawerHistoryEntries.Add(historyEntry);
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

                // Create a drawer transaction for the cash out
                var drawerTransaction = new DrawerTransaction
                {
                    DrawerId = drawer.DrawerId,
                    Timestamp = DateTime.Now,
                    Type = "Cash Out",
                    Amount = cashOutAmount,
                    Balance = drawer.CurrentBalance,
                    ActionType = "Cash Out",
                    Description = notes,
                    TransactionReference = drawer.DrawerId.ToString(),
                    IsVoided = false,
                    PaymentMethod = "Cash"
                };

                _dbContext.DrawerTransactions.Add(drawerTransaction);

                // Create a drawer history entry
                var historyEntry = new DrawerHistoryEntry
                {
                    Timestamp = DateTime.Now,
                    ActionType = "Cash Out",
                    Description = notes,
                    Amount = cashOutAmount,
                    ResultingBalance = drawer.CurrentBalance,
                    UserId = drawer.CashierId
                };

                _dbContext.DrawerHistoryEntries.Add(historyEntry);

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

                // Calculate difference between expected and closing balance
                decimal expectedBalance = drawer.OpeningBalance + drawer.CashIn - drawer.CashOut + drawer.TotalSales - drawer.TotalExpenses - drawer.TotalSupplierPayments;
                decimal difference = closingBalance - expectedBalance;

                drawer.CurrentBalance = closingBalance;
                drawer.ClosedAt = DateTime.Now;
                drawer.LastUpdated = DateTime.Now;
                drawer.Status = "Closed";

                // Update notes - append closing notes to existing notes
                string closingNote = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Drawer closed with balance: ${closingBalance:F2}";
                if (Math.Abs(difference) > 0.01m)
                {
                    string differenceNote = difference > 0
                        ? $" (Overage: ${difference:F2})"
                        : $" (Shortage: ${Math.Abs(difference):F2})";
                    closingNote += differenceNote;
                }

                if (!string.IsNullOrEmpty(closingNotes))
                {
                    closingNote += $" - {closingNotes}";
                }

                if (string.IsNullOrEmpty(drawer.Notes))
                    drawer.Notes = closingNote;
                else
                    drawer.Notes = $"{drawer.Notes}\n{closingNote}";

                // Create a drawer transaction for closing
                var drawerTransaction = new DrawerTransaction
                {
                    DrawerId = drawer.DrawerId,
                    Timestamp = DateTime.Now,
                    Type = "Close",
                    Amount = closingBalance,
                    Balance = closingBalance,
                    ActionType = "Close",
                    Description = closingNotes,
                    TransactionReference = drawer.DrawerId.ToString(),
                    IsVoided = false,
                    PaymentMethod = "Cash"
                };

                _dbContext.DrawerTransactions.Add(drawerTransaction);

                // Create a drawer history entry
                var historyEntry = new DrawerHistoryEntry
                {
                    Timestamp = DateTime.Now,
                    ActionType = "Close",
                    Description = $"Drawer closed with balance: ${closingBalance:F2} {(string.IsNullOrEmpty(closingNotes) ? "" : $" - {closingNotes}")}",
                    Amount = closingBalance,
                    ResultingBalance = closingBalance,
                    UserId = drawer.CashierId
                };

                _dbContext.DrawerHistoryEntries.Add(historyEntry);

                // If there's a significant difference, create an adjustment entry
                if (Math.Abs(difference) > 0.01m)
                {
                    var adjustmentEntry = new DrawerHistoryEntry
                    {
                        Timestamp = DateTime.Now,
                        ActionType = difference > 0 ? "Overage" : "Shortage",
                        Description = difference > 0
                            ? $"Cash overage of ${difference:F2}"
                            : $"Cash shortage of ${Math.Abs(difference):F2}",
                        Amount = Math.Abs(difference),
                        ResultingBalance = closingBalance,
                        UserId = drawer.CashierId
                    };

                    _dbContext.DrawerHistoryEntries.Add(adjustmentEntry);
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
        /// Gets the drawer transactions for a specific drawer
        /// </summary>
        /// <param name="drawerId">The drawer ID</param>
        /// <returns>A list of drawer transactions</returns>
        public async Task<List<DrawerTransaction>> GetDrawerTransactionsAsync(int drawerId)
        {
            try
            {
                return await _dbContext.DrawerTransactions
                    .Where(dt => dt.DrawerId == drawerId)
                    .OrderByDescending(dt => dt.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting drawer transactions: {ex.Message}");
                return new List<DrawerTransaction>();
            }
        }

        /// <summary>
        /// Gets the drawer history entries for a specific drawer
        /// </summary>
        /// <param name="drawerId">The drawer ID</param>
        /// <returns>A list of drawer history entries</returns>
        public async Task<List<DrawerHistoryEntry>> GetDrawerHistoryAsync(string cashierId)
        {
            try
            {
                return await _dbContext.DrawerHistoryEntries
                    .Where(dhe => dhe.UserId == cashierId)
                    .OrderByDescending(dhe => dhe.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting drawer history: {ex.Message}");
                return new List<DrawerHistoryEntry>();
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

                // Create drawer transactions if needed
                if (salesAmount > 0)
                {
                    var saleTransaction = new DrawerTransaction
                    {
                        DrawerId = drawer.DrawerId,
                        Timestamp = DateTime.Now,
                        Type = "Cash Sale",
                        Amount = salesAmount,
                        Balance = drawer.CurrentBalance,
                        ActionType = "Sale",
                        Description = "Sale added to drawer",
                        TransactionReference = "",
                        IsVoided = false,
                        PaymentMethod = "Cash"
                    };

                    _dbContext.DrawerTransactions.Add(saleTransaction);
                }

                if (expensesAmount > 0)
                {
                    var expenseTransaction = new DrawerTransaction
                    {
                        DrawerId = drawer.DrawerId,
                        Timestamp = DateTime.Now,
                        Type = "Expense",
                        Amount = expensesAmount,
                        Balance = drawer.CurrentBalance,
                        ActionType = "Expense",
                        Description = "Expense deducted from drawer",
                        TransactionReference = "",
                        IsVoided = false,
                        PaymentMethod = "Cash"
                    };

                    _dbContext.DrawerTransactions.Add(expenseTransaction);
                }

                if (supplierPayments > 0)
                {
                    var supplierTransaction = new DrawerTransaction
                    {
                        DrawerId = drawer.DrawerId,
                        Timestamp = DateTime.Now,
                        Type = "Supplier Payment",
                        Amount = supplierPayments,
                        Balance = drawer.CurrentBalance,
                        ActionType = "Supplier Payment",
                        Description = "Supplier payment deducted from drawer",
                        TransactionReference = "",
                        IsVoided = false,
                        PaymentMethod = "Cash"
                    };

                    _dbContext.DrawerTransactions.Add(supplierTransaction);
                }

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