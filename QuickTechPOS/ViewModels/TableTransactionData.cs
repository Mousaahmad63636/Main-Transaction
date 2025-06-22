using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickTechPOS.Models
{
    /// <summary>
    /// Data structure to store table-specific transaction information
    /// Used for multi-table POS operations where each table maintains its own transaction state
    /// </summary>
    public class TableTransactionData
    {
        /// <summary>
        /// Collection of cart items for this table
        /// </summary>
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        /// <summary>
        /// Customer ID associated with this table's transaction
        /// </summary>
        public int CustomerId { get; set; } = 0;

        /// <summary>
        /// Customer name for this table's transaction
        /// </summary>
        public string CustomerName { get; set; } = "Walk-in Customer";

        /// <summary>
        /// Selected customer object for this table
        /// </summary>
        public Customer SelectedCustomer { get; set; }

        /// <summary>
        /// Amount paid for this table's transaction
        /// </summary>
        public decimal PaidAmount { get; set; } = 0;

        /// <summary>
        /// Whether to add unpaid amount to customer debt
        /// </summary>
        public bool AddToCustomerDebt { get; set; } = false;

        /// <summary>
        /// Amount to add to customer debt
        /// </summary>
        public decimal AmountToDebt { get; set; } = 0;

        /// <summary>
        /// Last activity timestamp for this table
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.Now;

        /// <summary>
        /// Additional notes for this table's transaction
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Gets the total number of items in the cart
        /// </summary>
        public int TotalItemCount => CartItems?.Count ?? 0;

        /// <summary>
        /// Gets the total value of all items in the cart
        /// </summary>
        public decimal TotalValue => CartItems?.Sum(item => item.Total) ?? 0;

        /// <summary>
        /// Gets whether this table has any items
        /// </summary>
        public bool HasItems => TotalItemCount > 0;

        /// <summary>
        /// Gets the calculated status based on item count
        /// </summary>
        public string CalculatedStatus => HasItems ? "Occupied" : "Available";

        /// <summary>
        /// Creates a summary string for this table's transaction
        /// </summary>
        /// <returns>Formatted summary string</returns>
        public string GetSummary()
        {
            var customer = string.IsNullOrEmpty(CustomerName) || CustomerName == "Walk-in Customer"
                ? ""
                : $" - {CustomerName}";

            return $"{TotalItemCount} items (${TotalValue:F2}){customer}";
        }

        /// <summary>
        /// Validates the transaction data
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            // Validate cart items
            if (CartItems != null)
            {
                foreach (var item in CartItems)
                {
                    if (item.Product == null)
                    {
                        errors.Add("Cart contains items with missing product information");
                        continue;
                    }

                    if (item.Quantity <= 0)
                    {
                        errors.Add($"Invalid quantity for {item.Product.Name}: {item.Quantity}");
                    }

                    if (item.UnitPrice < 0)
                    {
                        errors.Add($"Invalid unit price for {item.Product.Name}: ${item.UnitPrice}");
                    }

                    if (item.Discount < 0)
                    {
                        errors.Add($"Invalid discount for {item.Product.Name}: ${item.Discount}");
                    }
                }
            }

            // Validate payment data
            if (PaidAmount < 0)
            {
                errors.Add("Paid amount cannot be negative");
            }

            if (AmountToDebt < 0)
            {
                errors.Add("Amount to debt cannot be negative");
            }

            if (AddToCustomerDebt && AmountToDebt == 0)
            {
                errors.Add("Amount to debt must be specified when adding to customer debt");
            }

            if (AddToCustomerDebt && CustomerId <= 0)
            {
                errors.Add("Customer must be selected when adding to customer debt");
            }

            return errors;
        }

        /// <summary>
        /// Clears all transaction data
        /// </summary>
        public void Clear()
        {
            CartItems?.Clear();
            CustomerId = 0;
            CustomerName = "Walk-in Customer";
            SelectedCustomer = null;
            PaidAmount = 0;
            AddToCustomerDebt = false;
            AmountToDebt = 0;
            Notes = string.Empty;
            LastActivity = DateTime.Now;
        }
    }
}