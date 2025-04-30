// File: QuickTechPOS/Models/Transaction.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTechPOS.Models
{
    /// <summary>
    /// Represents a sales transaction
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Unique identifier for the transaction
        /// </summary>
        [Key]
        public int TransactionId { get; set; }

        /// <summary>
        /// Customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Customer name
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Total transaction amount
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Amount paid by the customer
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Date and time of the transaction
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Type of transaction (e.g., Sale, Return)
        /// </summary>
        [MaxLength(450)]
        public string TransactionType { get; set; }

        /// <summary>
        /// Status of the transaction (e.g., Completed, Pending)
        /// </summary>
        [MaxLength(450)]
        public string Status { get; set; }

        /// <summary>
        /// Method of payment (e.g., Cash, Credit Card)
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Cashier/employee identifier
        /// </summary>
        public string CashierId { get; set; }

        /// <summary>
        /// Cashier/employee name
        /// </summary>
        public string CashierName { get; set; }

        /// <summary>
        /// Cashier/employee role
        /// </summary>
        public string CashierRole { get; set; }

        /// <summary>
        /// Gets or sets the collection of transaction details
        /// </summary>
        [NotMapped]
        public virtual ICollection<TransactionDetail> Details { get; set; } = new List<TransactionDetail>();

        /// <summary>
        /// Gets the change amount to be returned to the customer
        /// </summary>
        [NotMapped]
        public decimal ChangeAmount => PaidAmount - TotalAmount;

        /// <summary>
        /// Gets the formatted transaction date
        /// </summary>
        [NotMapped]
        public string FormattedDate => TransactionDate.ToString("yyyy-MM-dd HH:mm:ss");
    }
}