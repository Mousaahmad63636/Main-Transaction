using QuickTechPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuickTechPOS.Helpers
{
    /// <summary>
    /// Helper class for creating deep copies of objects to prevent reference sharing issues
    /// between tables in multi-table POS operations
    /// </summary>
    public static class DeepCopyHelper
    {
        /// <summary>
        /// Creates a deep copy of a single CartItem to prevent reference sharing
        /// </summary>
        /// <param name="source">Source cart item to copy</param>
        /// <returns>New CartItem instance with copied values</returns>
        public static CartItem DeepCopy(CartItem source)
        {
            if (source == null)
                return null;

            return new CartItem
            {
                // Core product and quantity information
                Product = source.Product, // Product reference can be shared as it's read-only
                Quantity = source.Quantity,
                UnitPrice = source.UnitPrice,

                // Discount and pricing information
                Discount = source.Discount,
                DiscountType = source.DiscountType,
                DiscountValue = source.DiscountValue,

                // Product variant flags
                IsBox = source.IsBox,
                IsWholesale = source.IsWholesale,

                // Note: Total property will be recalculated automatically via CartItem.Total getter
            };
        }

        /// <summary>
        /// Creates a deep copy of a collection of CartItems
        /// </summary>
        /// <param name="source">Source cart items collection</param>
        /// <returns>New List with deep copied cart items</returns>
        public static List<CartItem> DeepCopyCartItems(IEnumerable<CartItem> source)
        {
            if (source == null)
                return new List<CartItem>();

            var copiedItems = new List<CartItem>();

            foreach (var item in source)
            {
                var copiedItem = DeepCopy(item);
                if (copiedItem != null)
                {
                    copiedItems.Add(copiedItem);
                }
            }

            return copiedItems;
        }

        /// <summary>
        /// Creates a deep copy of TableTransactionData
        /// </summary>
        /// <param name="source">Source table transaction data</param>
        /// <returns>New TableTransactionData instance with deep copied values</returns>
        public static TableTransactionData DeepCopy(TableTransactionData source)
        {
            if (source == null)
                return null;

            return new TableTransactionData
            {
                CartItems = DeepCopyCartItems(source.CartItems),
                CustomerId = source.CustomerId,
                CustomerName = source.CustomerName,
                SelectedCustomer = source.SelectedCustomer, // Customer reference can be shared
                PaidAmount = source.PaidAmount,
                AddToCustomerDebt = source.AddToCustomerDebt,
                AmountToDebt = source.AmountToDebt,
                LastActivity = source.LastActivity,
                Notes = source.Notes
            };
        }

        /// <summary>
        /// Validates that two cart items are deeply equal (for testing purposes)
        /// </summary>
        /// <param name="item1">First cart item</param>
        /// <param name="item2">Second cart item</param>
        /// <returns>True if deeply equal, false otherwise</returns>
        public static bool AreCartItemsEqual(CartItem item1, CartItem item2)
        {
            if (item1 == null && item2 == null)
                return true;

            if (item1 == null || item2 == null)
                return false;

            return item1.Product?.ProductId == item2.Product?.ProductId &&
                   item1.Quantity == item2.Quantity &&
                   item1.UnitPrice == item2.UnitPrice &&
                   item1.Discount == item2.Discount &&
                   item1.DiscountType == item2.DiscountType &&
                   item1.DiscountValue == item2.DiscountValue &&
                   item1.IsBox == item2.IsBox &&
                   item1.IsWholesale == item2.IsWholesale;
        }

        /// <summary>
        /// Ensures cart items are properly isolated from each other
        /// </summary>
        /// <param name="cartItems">Cart items to validate</param>
        /// <returns>List of validation errors</returns>
        public static List<string> ValidateCartItemIsolation(List<CartItem> cartItems)
        {
            var errors = new List<string>();

            if (cartItems == null || cartItems.Count == 0)
                return errors;

            // Check for duplicate references (should not happen with proper deep copying)
            var seenReferences = new HashSet<object>();
            foreach (var item in cartItems)
            {
                if (seenReferences.Contains(item))
                {
                    errors.Add("Duplicate cart item reference detected - deep copying failed");
                }
                seenReferences.Add(item);
            }

            return errors;
        }
    }
}