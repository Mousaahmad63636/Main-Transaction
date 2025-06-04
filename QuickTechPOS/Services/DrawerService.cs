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
    public class DrawerService
    {
        private readonly DatabaseContext _dbContext;

        public DrawerService()
        {
            _dbContext = new DatabaseContext(ConfigurationService.ConnectionString);
        }

        // File: QuickTechPOS/Services/DrawerService.cs

        public async Task<Drawer> OpenDrawerAsync(string cashierId, string cashierName, decimal openingBalance, string notes = "")
        {
            try
            {
                Console.WriteLine($"[DrawerService] OpenDrawerAsync started - CashierID: {cashierId}, OpeningBalance: ${openingBalance:F2}");

                // Check for an existing open drawer first
                var existingOpenDrawer = await _dbContext.Drawers
                    .Where(d => d.CashierId == cashierId && d.Status == "Open")
                    .FirstOrDefaultAsync();

                if (existingOpenDrawer != null)
                {
                    Console.WriteLine($"[DrawerService] Found existing open drawer: ID={existingOpenDrawer.DrawerId}");
                    return existingOpenDrawer;
                }

                Console.WriteLine("[DrawerService] No existing open drawer found. Creating new drawer...");

                // CRITICAL: Follow the working pattern - create drawer without transaction
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
                    Notes = notes ?? string.Empty
                };

                // IMPORTANT: Save each operation independently as in the working version
                _dbContext.Drawers.Add(drawer);
                Console.WriteLine("[DrawerService] Saving drawer to database...");
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"[DrawerService] Drawer saved with ID: {drawer.DrawerId}");

                // Create and save transaction record
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
                Console.WriteLine("[DrawerService] Saving drawer transaction...");
                await _dbContext.SaveChangesAsync();
                Console.WriteLine("[DrawerService] Drawer transaction saved");

                // Create and save history entry
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
                Console.WriteLine("[DrawerService] Saving history entry...");
                await _dbContext.SaveChangesAsync();
                Console.WriteLine("[DrawerService] History entry saved");

                // Verify drawer was created properly
                Console.WriteLine("[DrawerService] Drawer opening completed successfully");
                return drawer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DrawerService] Error in OpenDrawerAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[DrawerService] Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"[DrawerService] Stack trace: {ex.StackTrace}");
                }
                throw;
            }
        }
        public void CheckDatabaseConnection()
        {
            try
            {
                Console.WriteLine("Testing database connection...");
                Console.WriteLine($"Connection string: {ConfigurationService.ConnectionString}");

                bool canConnect = _dbContext.Database.CanConnect();
                Console.WriteLine($"Can connect: {canConnect}");

                if (canConnect)
                {
                    try
                    {
                        var drawerCount = _dbContext.Drawers.Count();
                        Console.WriteLine($"Total drawers in database: {drawerCount}");

                        var drawerIds = _dbContext.Drawers.Select(d => d.DrawerId).Take(5).ToList();
                        Console.WriteLine($"Sample drawer IDs: {string.Join(", ", drawerIds)}");

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

        public async Task<Drawer> PerformCashOutAsync(int drawerId, decimal cashOutAmount, string notes)
        {
            try
            {
                Console.WriteLine($"Starting cash out operation for drawer #{drawerId}");
                Console.WriteLine($"Cash out amount: ${cashOutAmount:F2}");
                Console.WriteLine($"Notes: {notes}");

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

                decimal initialCashOut = drawer.CashOut;
                decimal initialBalance = drawer.CurrentBalance;

                drawer.CashOut += cashOutAmount;
                drawer.CurrentBalance -= cashOutAmount;
                drawer.NetCashFlow = drawer.TotalSales - drawer.TotalExpenses - drawer.TotalSupplierPayments - drawer.CashOut + drawer.CashIn;
                drawer.LastUpdated = DateTime.Now;

                string cashOutNote = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Cash Out: ${cashOutAmount:F2} - {notes}";
                drawer.Notes = string.IsNullOrEmpty(drawer.Notes)
                    ? cashOutNote
                    : $"{drawer.Notes}\n{cashOutNote}";

                try
                {
                    Console.WriteLine("Saving drawer changes...");
                    _dbContext.Drawers.Update(drawer);
                    await _dbContext.SaveChangesAsync();
                    Console.WriteLine("Drawer updated successfully");
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"ERROR updating drawer: {dbEx.Message}");

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

                try
                {
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
                }

                try
                {
                    var historyEntry = new DrawerHistoryEntry
                    {
                        Timestamp = DateTime.Now,
                        ActionType = "Cash Out",
                        Description = notes,
                        Amount = cashOutAmount,
                        ResultingBalance = drawer.CurrentBalance,
                        UserId = drawer.CashierId
                    };

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

                throw;
            }
        }
        /// <summary>
        /// Performs a cash in operation for the specified drawer
        /// </summary>
        /// <param name="drawerId">The ID of the drawer to add cash to</param>
        /// <param name="cashInAmount">The amount of cash to add</param>
        /// <param name="notes">Notes describing the reason for cash in</param>
        /// <returns>The updated drawer object</returns>
        public async Task<Drawer> PerformCashInAsync(int drawerId, decimal cashInAmount, string notes)
        {
            try
            {
                Console.WriteLine($"Starting cash in operation for drawer #{drawerId}");
                Console.WriteLine($"Cash in amount: ${cashInAmount:F2}");
                Console.WriteLine($"Notes: {notes}");

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

                if (drawer.Status != "Open")
                {
                    throw new InvalidOperationException("Cannot perform cash in on a drawer that is not open.");
                }

                if (cashInAmount <= 0)
                {
                    throw new ArgumentException("Cash in amount must be greater than zero.");
                }

                decimal initialCashIn = drawer.CashIn;
                decimal initialBalance = drawer.CurrentBalance;

                // Update drawer amounts
                drawer.CashIn += cashInAmount;
                drawer.CurrentBalance += cashInAmount;
                drawer.NetCashFlow = drawer.TotalSales - drawer.TotalExpenses - drawer.TotalSupplierPayments - drawer.CashOut + drawer.CashIn;
                drawer.LastUpdated = DateTime.Now;

                // Add notes
                string cashInNote = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Cash In: ${cashInAmount:F2} - {notes}";
                drawer.Notes = string.IsNullOrEmpty(drawer.Notes)
                    ? cashInNote
                    : $"{drawer.Notes}\n{cashInNote}";

                try
                {
                    Console.WriteLine("Saving drawer changes...");
                    _dbContext.Drawers.Update(drawer);
                    await _dbContext.SaveChangesAsync();
                    Console.WriteLine("Drawer updated successfully");
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"ERROR updating drawer: {dbEx.Message}");

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

                // Create drawer transaction record
                try
                {
                    var drawerTransaction = new DrawerTransaction
                    {
                        DrawerId = drawer.DrawerId,
                        Timestamp = DateTime.Now,
                        Type = "Cash In",
                        Amount = cashInAmount,
                        Balance = drawer.CurrentBalance,
                        ActionType = "Cash In",
                        Description = notes,
                        TransactionReference = drawer.DrawerId.ToString(),
                        IsVoided = false,
                        PaymentMethod = "Cash"
                    };

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
                }

                // Create history entry
                try
                {
                    var historyEntry = new DrawerHistoryEntry
                    {
                        Timestamp = DateTime.Now,
                        ActionType = "Cash In",
                        Description = notes,
                        Amount = cashInAmount,
                        ResultingBalance = drawer.CurrentBalance,
                        UserId = drawer.CashierId
                    };

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
                }

                Console.WriteLine($"Cash in completed successfully:");
                Console.WriteLine($"  - Drawer ID: {drawer.DrawerId}");
                Console.WriteLine($"  - CashIn: ${initialCashIn:F2} → ${drawer.CashIn:F2} (Δ ${cashInAmount:F2})");
                Console.WriteLine($"  - Balance: ${initialBalance:F2} → ${drawer.CurrentBalance:F2} (Δ +${cashInAmount:F2})");

                return drawer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in PerformCashInAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                throw;
            }
        }
        public async Task<Drawer> CloseDrawerAsync(int drawerId, decimal closingBalance, string closingNotes = "")
        {
            try
            {
                Console.WriteLine($"Starting close drawer operation for drawer #{drawerId}");

                // Load drawer without using transaction
                var drawer = await _dbContext.Drawers.FindAsync(drawerId);
                if (drawer == null)
                {
                    Console.WriteLine($"ERROR: Drawer with ID {drawerId} not found");
                    throw new ArgumentException($"Drawer with ID {drawerId} not found.");
                }

                // Verify drawer status
                Console.WriteLine($"Found drawer. Current status: {drawer.Status}");
                if (drawer.Status != "Open")
                {
                    Console.WriteLine($"ERROR: Cannot close drawer that is not open (status: {drawer.Status})");
                    throw new InvalidOperationException("Cannot close a drawer that is not open.");
                }

                // Calculate difference
                decimal expectedBalance = drawer.CurrentBalance;
                decimal difference = closingBalance - expectedBalance;
                Console.WriteLine($"Expected balance: ${expectedBalance:F2}, Closing balance: ${closingBalance:F2}, Difference: ${difference:F2}");

                // Update drawer properties - critical step
                drawer.CurrentBalance = closingBalance;
                drawer.ClosedAt = DateTime.Now;
                drawer.LastUpdated = DateTime.Now;
                drawer.Status = "Closed";  // This is critical

                // Prepare closing note
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

                // Handle notes with null check
                drawer.Notes = string.IsNullOrEmpty(drawer.Notes)
                    ? closingNote
                    : $"{drawer.Notes}\n{closingNote}";

                // CRITICAL: Save drawer changes first, separately
                try
                {
                    _dbContext.Entry(drawer).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                    Console.WriteLine("Drawer successfully updated to Closed status");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR saving drawer changes: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    throw;
                }

                // Now create and save transaction separately
                try
                {
                    var drawerTransaction = new DrawerTransaction
                    {
                        DrawerId = drawer.DrawerId,
                        Timestamp = DateTime.Now,
                        Type = "Close",
                        Amount = closingBalance,
                        Balance = closingBalance,
                        ActionType = "Close",
                        Description = closingNotes ?? string.Empty,
                        TransactionReference = drawer.DrawerId.ToString(),
                        IsVoided = false,
                        PaymentMethod = "Cash"
                    };

                    _dbContext.DrawerTransactions.Add(drawerTransaction);
                    await _dbContext.SaveChangesAsync();
                    Console.WriteLine("Drawer transaction added successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Failed to add drawer transaction: {ex.Message}");
                    // Continue - we already closed the drawer
                }

                // Add history entry separately
                try
                {
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
                    await _dbContext.SaveChangesAsync();
                    Console.WriteLine("History entry added successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Failed to add history entry: {ex.Message}");
                    // Continue - we already closed the drawer
                }

                // Verify the update
                try
                {
                    var verifyDrawer = await _dbContext.Drawers.FindAsync(drawerId);
                    if (verifyDrawer != null && verifyDrawer.Status == "Closed")
                    {
                        Console.WriteLine("Drawer status verification successful");
                    }
                    else if (verifyDrawer != null)
                    {
                        Console.WriteLine($"WARNING: Drawer status verification found status: {verifyDrawer.Status}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Verification check failed: {ex.Message}");
                }

                Console.WriteLine($"Drawer #{drawerId} closed successfully");
                return drawer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRITICAL ERROR in CloseDrawerAsync: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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

                var drawer = await _dbContext.Drawers.FindAsync(drawerId);
                report.AppendLine($"Drawer exists: {drawer != null}");

                if (drawer != null)
                {
                    report.AppendLine($"Drawer ID: {drawer.DrawerId}");
                    report.AppendLine($"Status: {drawer.Status}");
                    report.AppendLine($"Current Balance: ${drawer.CurrentBalance:F2}");
                    report.AppendLine($"CashOut: ${drawer.CashOut:F2}");

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
                var canConnect = await _dbContext.Database.CanConnectAsync();

                if (canConnect)
                {
                    var drawerCount = await _dbContext.Drawers.CountAsync();
                    Console.WriteLine($"Database connection successful. Drawer count: {drawerCount}");

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

        // File: QuickTechPOS/Services/DrawerService.cs

        public async Task<Drawer> GetOpenDrawerAsync(string cashierId)
        {
            try
            {
                Console.WriteLine($"[DrawerService] Getting open drawer for cashier ID: {cashierId}");

                // Use a new DbContext to avoid caching issues
                using var freshContext = new DatabaseContext(ConfigurationService.ConnectionString);

                var drawer = await freshContext.Drawers
                    .Where(d => d.CashierId == cashierId && d.Status == "Open")
                    .FirstOrDefaultAsync();

                if (drawer != null)
                {
                    Console.WriteLine($"[DrawerService] Found open drawer: ID={drawer.DrawerId}, Status={drawer.Status}, OpenedAt={drawer.OpenedAt}");

                    // Ensure Notes isn't null to avoid binding errors
                    if (drawer.Notes == null)
                    {
                        drawer.Notes = string.Empty;
                    }

                    // Double-check status to make absolutely sure
                    if (drawer.Status != "Open")
                    {
                        Console.WriteLine($"[DrawerService] WARNING: Found drawer has incorrect status: {drawer.Status}");
                    }
                }
                else
                {
                    Console.WriteLine("[DrawerService] No open drawer found");

                    // For debugging, check if there are ANY drawers for this cashier
                    var anyDrawers = await freshContext.Drawers
                        .Where(d => d.CashierId == cashierId)
                        .ToListAsync();

                    if (anyDrawers.Any())
                    {
                        Console.WriteLine($"[DrawerService] Found {anyDrawers.Count} drawer(s) for this cashier with non-open status:");
                        foreach (var d in anyDrawers)
                        {
                            Console.WriteLine($"  - Drawer #{d.DrawerId}: Status={d.Status}, OpenedAt={d.OpenedAt}, ClosedAt={d.ClosedAt}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[DrawerService] No drawers found for this cashier at all");
                    }
                }

                return drawer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DrawerService] Error in GetOpenDrawerAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[DrawerService] Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"[DrawerService] Stack trace: {ex.StackTrace}");
                }
                throw;
            }
        }
        // File: QuickTechPOS/Services/DrawerService.cs

        /// <summary>
        /// Ensures that pending database operations are flushed
        /// </summary>
        public async Task DrainPendingOperationsAsync()
        {
            try
            {
                Console.WriteLine("[DrawerService] Draining pending database operations");

                // Simply run a quick query to force any pending operations to complete
                var count = await _dbContext.Drawers.CountAsync();

                Console.WriteLine($"[DrawerService] Database drain completed, found {count} drawers");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DrawerService] Error draining pending operations: {ex.Message}");
            }
        }

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
        public async Task<Drawer> GetDrawerByIdAsync(int drawerId)
        {
            try
            {
                Console.WriteLine($"Getting drawer by ID: {drawerId}");

                // Create a new context to ensure we get fresh data
                using var freshContext = new DatabaseContext(ConfigurationService.ConnectionString);

                var drawer = await freshContext.Drawers.FindAsync(drawerId);

                if (drawer == null)
                {
                    Console.WriteLine($"No drawer found with ID: {drawerId}");
                    return null;
                }

                Console.WriteLine($"Found drawer #{drawerId}, Status: {drawer.Status}");

                // Ensure Notes isn't null to avoid binding errors
                if (drawer.Notes == null)
                {
                    drawer.Notes = string.Empty;
                }

                return drawer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDrawerByIdAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
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

                drawer.DailySales += salesAmount;
                drawer.TotalSales += salesAmount;

                drawer.DailyExpenses += expensesAmount;
                drawer.TotalExpenses += expensesAmount;

                drawer.DailySupplierPayments += supplierPayments;
                drawer.TotalSupplierPayments += supplierPayments;

                decimal cashInflow = salesAmount;
                decimal cashOutflow = expensesAmount + supplierPayments;
                drawer.NetCashFlow = drawer.CashIn - drawer.CashOut + drawer.TotalSales - drawer.TotalExpenses - drawer.TotalSupplierPayments;
                drawer.CurrentBalance += (cashInflow - cashOutflow);
                drawer.NetSales = drawer.TotalSales - drawer.TotalExpenses;

                drawer.LastUpdated = DateTime.Now;

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