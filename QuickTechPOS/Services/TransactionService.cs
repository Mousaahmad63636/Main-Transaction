// File: QuickTechPOS/Services/TransactionService.cs
using QuickTechPOS.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace QuickTechPOS.Services
{
    /// <summary>
    /// Provides operations for managing transactions
    /// </summary>
    public class TransactionService
    {
        private readonly DatabaseContext _dbContext;
        private readonly ProductService _productService;

        /// <summary>
        /// Initializes a new instance of the transaction service
        /// </summary>
        public TransactionService()
        {
            _dbContext = new DatabaseContext(ConfigurationService.ConnectionString);
            _productService = new ProductService();
        }

        /// <summary>
        /// Creates a new transaction with the given items
        /// </summary>
        /// <param name="items">The cart items</param>
        /// <param name="paidAmount">The amount paid by the customer</param>
        /// <param name="cashier">The cashier/employee who processed the transaction</param>
        /// <param name="paymentMethod">The payment method used</param>
        /// <param name="customerName">The customer name</param>
        /// <param name="customerId">The customer ID</param>
        /// <returns>The created transaction</returns>
        public async Task<Transaction> CreateTransactionAsync(
        List<CartItem> items,
        decimal paidAmount,
        Employee cashier,
        string paymentMethod = "Cash",
        string customerName = "Walk-in Customer",
        int customerId = 0)
        {
            // Start a stopwatch for performance monitoring
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine($"Starting transaction creation at: {DateTime.Now}");

            // Begin database transaction
            using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Calculate total amount
                decimal totalAmount = items.Sum(item => item.Total);
                Console.WriteLine($"Calculated total amount: {totalAmount:C2} from {items.Count} items");

                // Create transaction record
                var newTransaction = new Transaction
                {
                    CustomerId = customerId,
                    CustomerName = customerName ?? "Walk-in Customer", // Ensure we have a default value
                    TotalAmount = totalAmount,
                    PaidAmount = paidAmount,
                    TransactionDate = DateTime.Now,
                    TransactionType = "Sale",
                    Status = "Completed",
                    PaymentMethod = paymentMethod ?? "Cash", // Ensure we have a default value
                    CashierId = cashier.EmployeeId.ToString(),
                    CashierName = cashier.FullName ?? "Unknown", // Ensure we have a default value
                    CashierRole = cashier.Role ?? "Cashier" // Ensure we have a default value
                };

                // Add to database and save to get the generated TransactionId
                _dbContext.Transactions.Add(newTransaction);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"Created transaction with ID: {newTransaction.TransactionId}");

                // Create transaction details for each item
                foreach (var item in items)
                {
                    // Validate the product exists
                    if (item.Product == null || item.Product.ProductId <= 0)
                    {
                        Console.WriteLine("Warning: Skipping invalid product in transaction detail");
                        continue;
                    }

                    // Create the transaction detail with careful null checking
                    var detail = new TransactionDetail
                    {
                        TransactionId = newTransaction.TransactionId,
                        ProductId = item.Product.ProductId,
                        Quantity = item.Quantity > 0 ? item.Quantity : 1, // Ensure quantity is positive
                        UnitPrice = item.UnitPrice >= 0 ? item.UnitPrice : 0, // Ensure price is non-negative
                        PurchasePrice = item.Product.PurchasePrice >= 0 ? item.Product.PurchasePrice : 0, // Ensure price is non-negative
                        Discount = item.Discount >= 0 ? item.Discount : 0, // Ensure discount is non-negative
                        Total = item.Total >= 0 ? item.Total : (item.Quantity * item.UnitPrice) // Calculate total if needed
                    };

                    _dbContext.TransactionDetails.Add(detail);
                    Console.WriteLine($"Added transaction detail for product: {item.Product.Name}, Qty: {item.Quantity}, Total: {item.Total:C2}");

                    try
                    {
                        // Update product stock in a separate try/catch to isolate stock update issues
                        bool stockUpdated = await _productService.UpdateStockAsync(item.Product.ProductId, item.Quantity);
                        if (!stockUpdated)
                        {
                            Console.WriteLine($"Warning: Failed to update stock for product {item.Product.ProductId}");
                        }
                    }
                    catch (Exception stockEx)
                    {
                        // Log stock update error but continue with transaction
                        Console.WriteLine($"Stock update error for product {item.Product.ProductId}: {stockEx.Message}");
                    }
                }

                // Save transaction details
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    // Log detailed information about the error
                    Console.WriteLine($"Database update error: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                    }

                    // Get validation errors if any
                    var validationErrors = dbEx.Entries
                        .SelectMany(entry => entry.Entity.GetType().GetProperties())
                        .Where(property => property.GetCustomAttributes(typeof(RequiredAttribute), false).Any())
                        .Select(property => property.Name);

                    if (validationErrors.Any())
                    {
                        Console.WriteLine($"Validation errors on properties: {string.Join(", ", validationErrors)}");
                    }

                    // Rethrow to allow proper error handling
                    throw;
                }

                // Commit the transaction
                await dbTransaction.CommitAsync();

                stopwatch.Stop();
                Console.WriteLine($"Transaction completed successfully in {stopwatch.ElapsedMilliseconds}ms");

                return newTransaction;
            }
            catch (Exception ex)
            {
                // Roll back transaction on error
                await dbTransaction.RollbackAsync();

                // Log detailed error information
                Console.WriteLine($"ERROR in CreateTransactionAsync: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner exception type: {ex.InnerException.GetType().Name}");
                }

                // Enhance the exception to provide more detail
                throw new Exception($"Error during checkout: {ex.Message}. Inner exception: {ex.InnerException?.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a transaction by ID with its details
        /// </summary>
        /// <param name="transactionId">The transaction ID</param>
        /// <returns>The transaction with details</returns>
        public async Task<Transaction> GetTransactionWithDetailsAsync(int transactionId)
        {
            try
            {
                // Get the transaction
                var transaction = await _dbContext.Transactions.FindAsync(transactionId);
                if (transaction == null)
                    return null;

                // Get the transaction details
                var details = await _dbContext.TransactionDetails
                    .Where(d => d.TransactionId == transactionId)
                    .ToListAsync();

                // Set the details collection
                transaction.Details = details;

                return transaction;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTransactionWithDetailsAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing transaction with new details
        /// </summary>
        /// <param name="transaction">The updated transaction</param>
        /// <param name="details">The updated transaction details</param>
        /// <returns>True if the update was successful, otherwise false</returns>
        public async Task<bool> UpdateTransactionAsync(Transaction transaction, List<TransactionDetail> details)
        {
            // Start a stopwatch for performance monitoring
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine($"Starting transaction update at: {DateTime.Now}");

            // Begin database transaction
            using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Find the existing transaction
                var existingTransaction = await _dbContext.Transactions.FindAsync(transaction.TransactionId);
                if (existingTransaction == null)
                {
                    Console.WriteLine($"Transaction #{transaction.TransactionId} not found for update");
                    return false;
                }

                // Update transaction properties
                existingTransaction.CustomerId = transaction.CustomerId;
                existingTransaction.CustomerName = transaction.CustomerName;
                existingTransaction.TotalAmount = transaction.TotalAmount;
                existingTransaction.Status = transaction.Status;
                // Don't update certain fields to preserve history
                // existingTransaction.TransactionDate = transaction.TransactionDate;
                // existingTransaction.CashierId = transaction.CashierId;

                // Save transaction changes
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"Updated transaction #{transaction.TransactionId}");

                // Delete existing transaction details
                var existingDetails = await _dbContext.TransactionDetails
                    .Where(d => d.TransactionId == transaction.TransactionId)
                    .ToListAsync();

                _dbContext.TransactionDetails.RemoveRange(existingDetails);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"Removed {existingDetails.Count} existing transaction details");

                // Create new transaction details
                foreach (var detail in details)
                {
                    _dbContext.TransactionDetails.Add(detail);
                    Console.WriteLine($"Added transaction detail for product: {detail.ProductId}, Qty: {detail.Quantity}, Total: {detail.Total:C2}");

                    // Update product stock - this is complex for updates
                    // For simplicity, we'll skip stock adjustment in updates
                    // In a real system, you'd calculate the difference and adjust accordingly
                }

                // Save transaction details and commit the transaction
                await _dbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                stopwatch.Stop();
                Console.WriteLine($"Transaction update completed successfully in {stopwatch.ElapsedMilliseconds}ms");

                return true;
            }
            catch (Exception ex)
            {
                // Roll back transaction on error
                await dbTransaction.RollbackAsync();

                // Log detailed error information
                Console.WriteLine($"ERROR in UpdateTransactionAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the ID of the next transaction (by ID) after the specified transaction
        /// </summary>
        /// <param name="currentTransactionId">The current transaction ID</param>
        /// <returns>The next transaction ID, or null if none exists</returns>
        public async Task<int?> GetNextTransactionIdAsync(int currentTransactionId)
        {
            try
            {
                var nextTransaction = await _dbContext.Transactions
                    .Where(t => t.TransactionId > currentTransactionId)
                    .OrderBy(t => t.TransactionId)
                    .FirstOrDefaultAsync();

                return nextTransaction?.TransactionId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetNextTransactionIdAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the ID of the previous transaction (by ID) before the specified transaction
        /// </summary>
        /// <param name="currentTransactionId">The current transaction ID</param>
        /// <returns>The previous transaction ID, or null if none exists</returns>
        public async Task<int?> GetPreviousTransactionIdAsync(int currentTransactionId)
        {
            try
            {
                var previousTransaction = await _dbContext.Transactions
                    .Where(t => t.TransactionId < currentTransactionId)
                    .OrderByDescending(t => t.TransactionId)
                    .FirstOrDefaultAsync();

                return previousTransaction?.TransactionId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPreviousTransactionIdAsync: {ex.Message}");
                return null;
            }
        }
    }
}