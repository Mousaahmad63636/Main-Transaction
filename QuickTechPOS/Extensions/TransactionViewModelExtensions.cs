using QuickTechPOS.Models;
using QuickTechPOS.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace QuickTechPOS.Extensions
{
    /// <summary>
    /// Extension methods for TransactionViewModel to support enhanced table navigation
    /// </summary>
    public static class TransactionViewModelExtensions
    {
        /// <summary>
        /// Gets a summary of all active table transactions
        /// </summary>
        /// <param name="viewModel">The transaction view model</param>
        /// <returns>Dictionary of table summaries keyed by table ID</returns>
        public static Dictionary<int, string> GetActiveTablesSummary(this TransactionViewModel viewModel)
        {
            var summary = new Dictionary<int, string>();

            if (viewModel.ActiveTables == null)
                return summary;

            foreach (var table in viewModel.ActiveTables)
            {
                // Create a summary for each active table
                var tableInfo = $"{table.DisplayName} - Active";
                summary[table.Id] = tableInfo;
            }

            return summary;
        }

        /// <summary>
        /// Checks if the view model has any unsaved table data
        /// </summary>
        /// <param name="viewModel">The transaction view model</param>
        /// <returns>True if there are unsaved changes, false otherwise</returns>
        public static bool HasUnsavedTableData(this TransactionViewModel viewModel)
        {
            // Check if current cart has items
            return viewModel.CartItems?.Count > 0;
        }

        /// <summary>
        /// Gets the table with the most recent activity
        /// </summary>
        /// <param name="viewModel">The transaction view model</param>
        /// <returns>Most recently active table or null</returns>
        public static RestaurantTable GetMostRecentTable(this TransactionViewModel viewModel)
        {
            // Return the first active table as a placeholder
            // In a full implementation, this would check LastActivity timestamps
            return viewModel.ActiveTables?.FirstOrDefault();
        }

        /// <summary>
        /// Validates that table navigation state is consistent
        /// </summary>
        /// <param name="viewModel">The transaction view model</param>
        /// <returns>List of validation issues, empty if valid</returns>
        public static List<string> ValidateTableNavigationState(this TransactionViewModel viewModel)
        {
            var issues = new List<string>();

            if (viewModel.SelectedTable != null && viewModel.ActiveTables != null)
            {
                var selectedTableInActive = viewModel.ActiveTables.Any(t => t.Id == viewModel.SelectedTable.Id);
                if (!selectedTableInActive)
                {
                    issues.Add("Selected table is not in active tables collection");
                }
            }

            if (viewModel.HasMultipleTables && (viewModel.ActiveTables?.Count ?? 0) <= 1)
            {
                issues.Add("HasMultipleTables is true but active tables count is <= 1");
            }

            return issues;
        }

        /// <summary>
        /// Gets navigation information for display purposes
        /// </summary>
        /// <param name="viewModel">The transaction view model</param>
        /// <returns>Formatted navigation information string</returns>
        public static string GetNavigationDisplayInfo(this TransactionViewModel viewModel)
        {
            if (viewModel.ActiveTables == null || viewModel.ActiveTables.Count == 0)
                return "No active tables";

            if (viewModel.ActiveTables.Count == 1)
                return $"1 table: {viewModel.ActiveTables.First().DisplayName}";

            var currentTableName = viewModel.SelectedTable?.DisplayName ?? "None selected";
            return $"{viewModel.ActiveTables.Count} tables active, current: {currentTableName}";
        }

        /// <summary>
        /// Calculates total transaction value across all active tables
        /// </summary>
        /// <param name="viewModel">The transaction view model</param>
        /// <returns>Total value of all active table transactions</returns>
        public static decimal GetTotalActiveTablesValue(this TransactionViewModel viewModel)
        {
            // This would require access to internal table transaction data
            // For now, return the current table's total
            return viewModel.TotalAmount;
        }

        /// <summary>
        /// Gets a list of table names for quick reference
        /// </summary>
        /// <param name="viewModel">The transaction view model</param>
        /// <returns>List of active table display names</returns>
        public static List<string> GetActiveTableNames(this TransactionViewModel viewModel)
        {
            return viewModel.ActiveTables?.Select(t => t.DisplayName).ToList() ?? new List<string>();
        }

        /// <summary>
        /// Checks if navigation to a specific table is possible
        /// </summary>
        /// <param name="viewModel">The transaction view model</param>
        /// <param name="tableId">Target table ID</param>
        /// <returns>True if navigation is possible, false otherwise</returns>
        public static bool CanNavigateToTable(this TransactionViewModel viewModel, int tableId)
        {
            return viewModel.ActiveTables?.Any(t => t.Id == tableId) ?? false;
        }

        /// <summary>
        /// Gets the index of a table in the active tables collection
        /// </summary>
        /// <param name="viewModel">The transaction view model</param>
        /// <param name="tableId">Table ID to find</param>
        /// <returns>Index of the table, or -1 if not found</returns>
        public static int GetTableIndex(this TransactionViewModel viewModel, int tableId)
        {
            if (viewModel.ActiveTables == null)
                return -1;

            for (int i = 0; i < viewModel.ActiveTables.Count; i++)
            {
                if (viewModel.ActiveTables[i].Id == tableId)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Creates a backup of current table state for recovery purposes
        /// </summary>
        /// <param name="viewModel">The transaction view model</param>
        /// <returns>Dictionary containing table state backup</returns>
        public static Dictionary<string, object> CreateTableStateBackup(this TransactionViewModel viewModel)
        {
            var backup = new Dictionary<string, object>();

            if (viewModel.SelectedTable != null)
            {
                backup["SelectedTableId"] = viewModel.SelectedTable.Id;
                backup["SelectedTableName"] = viewModel.SelectedTable.DisplayName;
            }

            backup["CartItemCount"] = viewModel.CartItems?.Count ?? 0;
            backup["TotalAmount"] = viewModel.TotalAmount;
            backup["CustomerName"] = viewModel.CustomerName;
            backup["CustomerId"] = viewModel.CustomerId;
            backup["PaidAmount"] = viewModel.PaidAmount;
            backup["AddToCustomerDebt"] = viewModel.AddToCustomerDebt;
            backup["Timestamp"] = System.DateTime.Now;

            return backup;
        }
    }
}