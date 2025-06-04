
using QuickTechPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickTechPOS.Helpers
{
    /// <summary>
    /// Helper class to ensure smooth integration of table navigation functionality
    /// Provides utility methods for table management and data validation
    /// </summary>
    public static class TableNavigationHelper
    {
        /// <summary>
        /// Validates that a table can be safely added to the active tables collection
        /// </summary>
        /// <param name="table">The table to validate</param>
        /// <param name="activeTables">Current active tables collection</param>
        /// <returns>True if table can be added, false otherwise</returns>
        public static bool CanAddTableToActive(RestaurantTable table, IEnumerable<RestaurantTable> activeTables)
        {
            if (table == null || !table.IsActive)
                return false;

            // Check if table is already in active collection
            return !activeTables.Any(t => t.Id == table.Id);
        }

        /// <summary>
        /// Creates a summary of table transaction data for display purposes
        /// </summary>
        /// <param name="table">The restaurant table</param>
        /// <param name="cartItems">Cart items for the table</param>
        /// <param name="customerName">Customer name</param>
        /// <returns>Formatted summary string</returns>
        public static string CreateTableSummary(RestaurantTable table, IEnumerable<CartItem> cartItems, string customerName)
        {
            if (table == null)
                return "Unknown Table";

            var itemCount = cartItems?.Count() ?? 0;
            var totalAmount = cartItems?.Sum(i => i.Total) ?? 0;
            var customer = string.IsNullOrEmpty(customerName) || customerName == "Walk-in Customer"
                ? ""
                : $" - {customerName}";

            return $"{table.DisplayName}: {itemCount} items (${totalAmount:F2}){customer}";
        }

        /// <summary>
        /// Validates cart item data before saving to table transaction data
        /// </summary>
        /// <param name="cartItems">Cart items to validate</param>
        /// <returns>List of validation errors, empty if valid</returns>
        public static List<string> ValidateCartItems(IEnumerable<CartItem> cartItems)
        {
            var errors = new List<string>();

            if (cartItems == null)
                return errors;

            foreach (var item in cartItems)
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

            return errors;
        }

        /// <summary>
        /// Safely copies cart items to prevent reference issues between tables
        /// </summary>
        /// <param name="sourceItems">Source cart items to copy</param>
        /// <returns>Deep copied cart items</returns>
        public static List<CartItem> DeepCopyCartItems(IEnumerable<CartItem> sourceItems)
        {
            var copiedItems = new List<CartItem>();

            if (sourceItems == null)
                return copiedItems;

            foreach (var sourceItem in sourceItems)
            {
                var copiedItem = new CartItem
                {
                    Product = sourceItem.Product,
                    Quantity = sourceItem.Quantity,
                    UnitPrice = sourceItem.UnitPrice,
                    Discount = sourceItem.Discount,
                    DiscountType = sourceItem.DiscountType,
                    IsBox = sourceItem.IsBox,
                    IsWholesale = sourceItem.IsWholesale
                };

                copiedItems.Add(copiedItem);
            }

            return copiedItems;
        }

        /// <summary>
        /// Generates a unique transaction reference for table-based transactions
        /// </summary>
        /// <param name="table">The restaurant table</param>
        /// <param name="timestamp">Transaction timestamp</param>
        /// <returns>Unique transaction reference string</returns>
        public static string GenerateTableTransactionReference(RestaurantTable table, DateTime timestamp)
        {
            if (table == null)
                return $"TXN-{timestamp:yyyyMMddHHmmss}";

            return $"T{table.TableNumber:D3}-{timestamp:yyyyMMddHHmmss}";
        }

        /// <summary>
        /// Formats table status for display with appropriate styling hints
        /// </summary>
        /// <param name="table">The restaurant table</param>
        /// <returns>Tuple with display text and status class</returns>
        public static (string DisplayText, string StatusClass) FormatTableStatus(RestaurantTable table)
        {
            if (table == null)
                return ("Unknown", "status-unknown");

            return table.Status?.ToLower() switch
            {
                "available" => ($"{table.DisplayName} (Available)", "status-available"),
                "occupied" => ($"{table.DisplayName} (Occupied)", "status-occupied"),
                "reserved" => ($"{table.DisplayName} (Reserved)", "status-reserved"),
                "out of service" => ($"{table.DisplayName} (Out of Service)", "status-outofservice"),
                _ => ($"{table.DisplayName} ({table.Status})", "status-unknown")
            };
        }

        /// <summary>
        /// Checks if a table navigation operation is safe to perform
        /// </summary>
        /// <param name="currentTableIndex">Current table index</param>
        /// <param name="targetIndex">Target table index</param>
        /// <param name="totalTables">Total number of active tables</param>
        /// <returns>True if navigation is safe, false otherwise</returns>
        public static bool IsSafeNavigation(int currentTableIndex, int targetIndex, int totalTables)
        {
            return targetIndex >= 0 &&
                   targetIndex < totalTables &&
                   targetIndex != currentTableIndex;
        }

        /// <summary>
        /// Creates a table activity log entry for audit purposes
        /// </summary>
        /// <param name="table">The restaurant table</param>
        /// <param name="action">Action performed</param>
        /// <param name="details">Additional details</param>
        /// <returns>Formatted log entry</returns>
        public static string CreateTableActivityLog(RestaurantTable table, string action, string details = "")
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var tableInfo = table?.DisplayName ?? "Unknown Table";
            var detailText = string.IsNullOrEmpty(details) ? "" : $" - {details}";

            return $"[{timestamp}] {tableInfo}: {action}{detailText}";
        }

        /// <summary>
        /// Calculates table utilization metrics for reporting
        /// </summary>
        /// <param name="activeTables">Collection of active tables</param>
        /// <param name="tableTransactionData">Dictionary of table transaction data</param>
        /// <returns>Dictionary of utilization metrics</returns>
        public static Dictionary<string, decimal> CalculateTableUtilization(
            IEnumerable<RestaurantTable> activeTables,
            IDictionary<int, object> tableTransactionData)
        {
            var metrics = new Dictionary<string, decimal>();

            if (activeTables == null || !activeTables.Any())
            {
                metrics["AverageItemsPerTable"] = 0;
                metrics["AverageAmountPerTable"] = 0;
                metrics["TableUtilizationRate"] = 0;
                return metrics;
            }

            var totalTables = activeTables.Count();
            var tablesWithItems = tableTransactionData?.Count ?? 0;

            metrics["TotalActiveTables"] = totalTables;
            metrics["TablesWithItems"] = tablesWithItems;
            metrics["TableUtilizationRate"] = totalTables > 0 ? (decimal)tablesWithItems / totalTables * 100 : 0;

            // Note: Additional calculations would require access to actual table transaction data
            // This is a placeholder for the structure
            metrics["AverageItemsPerTable"] = 0;
            metrics["AverageAmountPerTable"] = 0;

            return metrics;
        }

        /// <summary>
        /// Validates table state consistency for debugging purposes
        /// </summary>
        /// <param name="selectedTable">Currently selected table</param>
        /// <param name="activeTables">Collection of active tables</param>
        /// <param name="currentTableIndex">Current table index</param>
        /// <returns>List of validation issues</returns>
        public static List<string> ValidateTableState(
            RestaurantTable selectedTable,
            IEnumerable<RestaurantTable> activeTables,
            int currentTableIndex)
        {
            var issues = new List<string>();

            if (selectedTable != null && activeTables != null)
            {
                var selectedTableInActive = activeTables.Any(t => t.Id == selectedTable.Id);
                if (!selectedTableInActive)
                {
                    issues.Add("Selected table is not in active tables collection");
                }

                var activeTablesList = activeTables.ToList();
                if (currentTableIndex >= 0 && currentTableIndex < activeTablesList.Count)
                {
                    var indexedTable = activeTablesList[currentTableIndex];
                    if (indexedTable.Id != selectedTable.Id)
                    {
                        issues.Add("Current table index does not match selected table");
                    }
                }
            }

            if (activeTables != null)
            {
                var activeTablesList = activeTables.ToList();
                if (currentTableIndex < -1 || currentTableIndex >= activeTablesList.Count)
                {
                    issues.Add($"Current table index {currentTableIndex} is out of range for {activeTablesList.Count} active tables");
                }
            }

            return issues;
        }
    }
}
