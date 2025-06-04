// File: QuickTechPOS/Models/Category.cs
using System.ComponentModel.DataAnnotations;

namespace QuickTechPOS.Models
{
    /// <summary>
    /// Represents a product or expense category in the system
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Unique identifier for the category
        /// </summary>
        [Key]
        public int CategoryId { get; set; }

        /// <summary>
        /// Name of the category
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the category
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Type of category - either "Product" or "Expense"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = "Product";

        /// <summary>
        /// Count of products in this category (computed field)
        /// </summary>
        public int ProductCount { get; set; }

        /// <summary>
        /// Gets a display name for the category including product count
        /// </summary>
        public string DisplayName => $"{Name} ({ProductCount})";

        /// <summary>
        /// Returns the category name for display purposes
        /// </summary>
        /// <returns>Category name</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}