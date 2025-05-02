// File: QuickTechPOS/Services/DrawerService.cs (key updated methods)

using QuickTechPOS.Models;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

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

        public void CheckDatabaseConnection()
        {
            try
            {
                Console.WriteLine("Testing database connection...");
                Console.WriteLine($"Connection string: {ConfigurationService.ConnectionString}");

                // First check if we can connect
                bool canConnect = _dbContext.Database.CanConnect();
                Console.WriteLine($"Can connect: {canConnect}");

                if (canConnect)
                {
                    try
                    {
                        // Try a simple query
                        var drawerCount = _dbContext.Drawers.Count();
                        Console.WriteLine($"Total drawers in database: {drawerCount}");

                        // Try specific drawer
                        var drawerIds = _dbContext.Drawers.Select(d => d.DrawerId).Take(5).ToList();
                        Console.WriteLine($"Sample drawer IDs: {string.Join(", ", drawerIds)}");

                        // Check schema
                        Console.WriteLine("Checking database schema...");
                        var drawerTableExists = _dbContext.Model.FindEntityType(typeof(Drawer)) != null;
                        var transactionTableExists = _dbContext.Model.FindEntityType(typeof(DrawerTransaction)) != null;
                        var historyTableExists = _dbContext.Model.FindEntityType(typeof(DrawerHistoryEntry)) != null;

                        Console.WriteLine($"Drawer table exists: {drawerTableExists}");
                        Console.WriteLine($"DrawerTransaction table exists: {transactionTableExists}");
                        Console.WriteLine($"DrawerHistoryEntry table exists: {historyTableExists}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing queries: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
            // Don't use transaction scope initially - let's see the raw error first
            try
            {
                Console.WriteLine($"Starting cash out operation for drawer #{drawerId}");
                Console.WriteLine($"Cash out amount: ${cashOutAmount:F2}");
                Console.WriteLine($"Notes: {notes}");

                // Load drawer with more detailed error handling
                Drawer? drawer = null;
                try
                {
                    drawer = await _dbContext.Drawers.FindAsync(drawerId);
                    Console.WriteLine($"Drawer found: {drawer != null}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR finding drawer: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    throw;
                }

                if (drawer == null)
                {
                    throw new ArgumentException($"Drawer with ID {drawerId} not found.");
                }

                // Basic validation
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

                // Save initial values for logging
                decimal initialCashOut = drawer.CashOut;
                decimal initialBalance = drawer.CurrentBalance;

                // Update cash out amount
                drawer.CashOut += cashOutAmount;

                // Update current balance
                drawer.CurrentBalance -= cashOutAmount;

                // Update net calculations
                drawer.NetCashFlow = drawer.TotalSales - drawer.TotalExpenses - drawer.TotalSupplierPayments - drawer.CashOut + drawer.CashIn;

                // Update timestamp
                drawer.LastUpdated = DateTime.Now;

                // Update notes - handle null notes
                string cashOutNote = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Cash Out: ${cashOutAmount:F2} - {notes}";
                drawer.Notes = string.IsNullOrEmpty(drawer.Notes)
                    ? cashOutNote
                    : $"{drawer.Notes}\n{cashOutNote}";

                // Separate try blocks to see which operation is failing
                try
                {
                    // First attempt to save drawer changes alone
                    Console.WriteLine("Saving drawer changes...");
                    _dbContext.Drawers.Update(drawer);  // Explicitly mark as updated
                    await _dbContext.SaveChangesAsync();
                    Console.WriteLine("Drawer updated successfully");
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"ERROR updating drawer: {dbEx.Message}");

                    // Log the SQL error details from inner exception 
                    if (dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                    {
                        Console.WriteLine($"SQL Error Number: {sqlEx.Number}");
                        Console.WriteLine($"SQL Error Message: {sqlEx.Message}");
                        Console.WriteLine($"SQL Server Error: {sqlEx.Server}");
                        Console.WriteLine($"SQL Line Number: {sqlEx.LineNumber}");
                    }

                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                        if (dbEx.InnerException.InnerException != null)
                        {
                            Console.WriteLine($"Inner inner exception: {dbEx.InnerException.InnerException.Message}");
                        }
                    }
                    throw;
                }

                // Now create and save transactions
                try
                {
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

                    // Add transaction
                    Console.WriteLine("Adding drawer transaction...");
                    _dbContext.DrawerTransactions.Add(drawerTransaction);
                    await _dbContext.SaveChangesAsync();
                    Console.WriteLine("Transaction added successfully");
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"ERROR adding transaction: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                    }
                    // Don't throw, let's try to continue with history entry
                }

                try
                {
                    // Create drawer history entry
                    var historyEntry = new DrawerHistoryEntry
                    {
                        Timestamp = DateTime.Now,
                        ActionType = "Cash Out",
                        Description = notes,
                        Amount = cashOutAmount,
                        ResultingBalance = drawer.CurrentBalance,
                        UserId = drawer.CashierId
                    };

                    // Add history entry
                    Console.WriteLine("Adding drawer history entry...");
                    _dbContext.DrawerHistoryEntries.Add(historyEntry);
                    await _dbContext.SaveChangesAsync();
                    Console.WriteLine("History entry added successfully");
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"ERROR adding history entry: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                    }
                    // Don't throw, we've already updated the drawer which is most important
                }

                Console.WriteLine($"Cash out completed successfully:");
                Console.WriteLine($"  - Drawer ID: {drawer.DrawerId}");
                Console.WriteLine($"  - CashOut: ${initialCashOut:F2} → ${drawer.CashOut:F2} (Δ ${cashOutAmount:F2})");
                Console.WriteLine($"  - Balance: ${initialBalance:F2} → ${drawer.CurrentBalance:F2} (Δ -${cashOutAmount:F2})");

                return drawer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in PerformCashOutAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                throw; // Re-throw the exception to be handled by the caller
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
        public async Task<string> DiagnoseCashOutIssueAsync(int drawerId)
        {
            var report = new StringBuilder();

            try
            {
                report.AppendLine($"===== CASH OUT DIAGNOSTIC REPORT =====");
                report.AppendLine($"Timestamp: {DateTime.Now}");

                // 1. Check if drawer exists
                var drawer = await _dbContext.Drawers.FindAsync(drawerId);
                report.AppendLine($"Drawer exists: {drawer != null}");

                if (drawer != null)
                {
                    report.AppendLine($"Drawer ID: {drawer.DrawerId}");
                    report.AppendLine($"Status: {drawer.Status}");
                    report.AppendLine($"Current Balance: ${drawer.CurrentBalance:F2}");
                    report.AppendLine($"CashOut: ${drawer.CashOut:F2}");

                    // 2. Check recent transactions
                    var recentTransactions = await _dbContext.DrawerTransactions
                        .Where(t => t.DrawerId == drawerId)
                        .OrderByDescending(t => t.Timestamp)
                        .Take(5)
                        .ToListAsync();

                    report.AppendLine($"\nRecent Transactions: {recentTransactions.Count}");
                    foreach (var tx in recentTransactions)
                    {
                        report.AppendLine($"  - {tx.Timestamp}: {tx.Type} ${tx.Amount:F2} ({tx.Description})");
                    }

                    // 3. Check DB connection
                    report.AppendLine($"\nDatabase Connection:");
                    try
                    {
                        bool canConnect = _dbContext.Database.CanConnect();
                        report.AppendLine($"  Can connect: {canConnect}");
                    }
                    catch (Exception ex)
                    {
                        report.AppendLine($"  Connection error: {ex.Message}");
                    }
                }

                return report.ToString();
            }
            catch (Exception ex)
            {
                report.AppendLine($"\nDIAGNOSTIC ERROR: {ex.Message}");
                return report.ToString();
            }
        }

        public async Task<bool> TestDatabaseConnectionAsync()
        {
            try
            {
                // Test specific to cash out functionality
                var canConnect = await _dbContext.Database.CanConnectAsync();

                if (canConnect)
                {
                    // Check if we can access Drawers table
                    var drawerCount = await _dbContext.Drawers.CountAsync();
                    Console.WriteLine($"Database connection successful. Drawer count: {drawerCount}");

                    // Check if we can access DrawerTransactions table
                    var transactionCount = await _dbContext.DrawerTransactions.CountAsync();
                    Console.WriteLine($"DrawerTransactions table accessible. Count: {transactionCount}");

                    return true;
                }
                else
                {
                    Console.WriteLine("Database connection failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection test error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<List<DrawerTransaction>> GetCashOutTransactionsAsync(int drawerId)
        {
            try
            {
                var cashOuts = await _dbContext.DrawerTransactions
                    .Where(dt => dt.DrawerId == drawerId && dt.Type == "Cash Out" && !dt.IsVoided)
                    .OrderByDescending(dt => dt.Timestamp)
                    .Take(10)
                    .ToListAsync();

                // Log found transactions
                Console.WriteLine($"Found {cashOuts.Count} cash out transactions for drawer {drawerId}:");
                foreach (var tx in cashOuts)
                {
                    Console.WriteLine($"  {tx.Timestamp}: ${tx.Amount:F2} - {tx.Description}");
                }

                return cashOuts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving cash out transactions: {ex.Message}");
                return new List<DrawerTransaction>();
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