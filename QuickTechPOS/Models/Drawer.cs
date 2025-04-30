using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTechPOS.Models
{
    /// <summary>
    /// Represents a cash drawer session
    /// </summary>
    public class Drawer
    {
        /// <summary>
        /// Unique identifier for the drawer session
        /// </summary>
        [Key]
        public int DrawerId { get; set; }

        /// <summary>
        /// Initial cash amount in the drawer
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal OpeningBalance { get; set; }

        /// <summary>
        /// Current cash amount in the drawer
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Additional cash added to the drawer
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CashIn { get; set; }

        /// <summary>
        /// Cash removed from the drawer
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CashOut { get; set; }

        /// <summary>
        /// Total sales for this drawer session
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalSales { get; set; }

        /// <summary>
        /// Total expenses for this drawer session
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalExpenses { get; set; }

        /// <summary>
        /// Total supplier payments for this drawer session
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalSupplierPayments { get; set; }

        /// <summary>
        /// Net cash flow for this drawer session
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetCashFlow { get; set; }

        /// <summary>
        /// Sales for the current day
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DailySales { get; set; }

        /// <summary>
        /// Expenses for the current day
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DailyExpenses { get; set; }

        /// <summary>
        /// Supplier payments for the current day
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DailySupplierPayments { get; set; }

        /// <summary>
        /// Date and time when the drawer was opened
        /// </summary>
        public DateTime OpenedAt { get; set; }

        /// <summary>
        /// Date and time when the drawer was closed
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// Date and time when the drawer was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Net sales (total sales minus expenses)
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal NetSales { get; set; }

        /// <summary>
        /// Status of the drawer (Open/Closed)
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; }

        /// <summary>
        /// Additional notes for this drawer session
        /// </summary>
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
        /// <summary>
        /// ID of the cashier who opened the drawer
        /// </summary>
        [MaxLength(50)]
        public string CashierId { get; set; }

        /// <summary>
        /// Name of the cashier who opened the drawer
        /// </summary>
        [MaxLength(100)]
        public string CashierName { get; set; }

        /// <summary>
        /// Gets the formatted opening balance with currency symbol
        /// </summary>
        [NotMapped]
        public string FormattedOpeningBalance => $"${OpeningBalance:F2}";

        /// <summary>
        /// Gets the formatted current balance with currency symbol
        /// </summary>
        [NotMapped]
        public string FormattedCurrentBalance => $"${CurrentBalance:F2}";

        /// <summary>
        /// Gets the formatted total sales with currency symbol
        /// </summary>
        [NotMapped]
        public string FormattedTotalSales => $"${TotalSales:F2}";

        /// <summary>
        /// Gets the formatted daily sales with currency symbol
        /// </summary>
        [NotMapped]
        public string FormattedDailySales => $"${DailySales:F2}";

        /// <summary>
        /// Gets the formatted net sales with currency symbol
        /// </summary>
        [NotMapped]
        public string FormattedNetSales => $"${NetSales:F2}";

        /// <summary>
        /// Gets the formatted opened at date and time
        /// </summary>
        [NotMapped]
        public string FormattedOpenedAt => OpenedAt.ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// Gets the formatted closed at date and time
        /// </summary>
        [NotMapped]
        public string FormattedClosedAt => ClosedAt.HasValue ? ClosedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "Not closed";
    }
}