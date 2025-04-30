// File: QuickTechPOS/Models/CartItem.cs

using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTechPOS.Models
{
    /// <summary>
    /// Represents an item in the shopping cart
    /// </summary>
    public class CartItem
    {
        /// <summary>
        /// Associated product
        /// </summary>
        public Product Product { get; set; }

        /// <summary>
        /// Quantity of the product
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Unit price of the product
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Discount amount for this item
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Discount type: 0 = Amount, 1 = Percentage
        /// </summary>
        [NotMapped]
        public int DiscountType { get; set; } = 0;

        /// <summary>
        /// Discount value (amount or percentage)
        /// </summary>
        [NotMapped]
        public decimal DiscountValue
        {
            get => DiscountType == 0 ? Discount : (Discount / Subtotal) * 100;
            set
            {
                if (DiscountType == 0)
                {
                    // Amount-based discount
                    Discount = value > Subtotal ? Subtotal : value;
                }
                else
                {
                    // Percentage-based discount
                    decimal percentage = value > 100 ? 100 : value;
                    Discount = (percentage / 100) * Subtotal;
                }
            }
        }

        /// <summary>
        /// Gets the subtotal for this item (Quantity * UnitPrice)
        /// </summary>
        [NotMapped]
        public decimal Subtotal => Quantity * UnitPrice;

        /// <summary>
        /// Gets the total amount for this item (Subtotal - Discount)
        /// </summary>
        [NotMapped]
        public decimal Total => Subtotal - Discount;
    }
}