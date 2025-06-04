// File: QuickTechPOS/Services/CategoryService.cs

using Microsoft.EntityFrameworkCore;
using QuickTechPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickTechPOS.Services
{
    /// <summary>
    /// Provides operations for managing product categories
    /// </summary>
    public class CategoryService
    {
        private readonly DatabaseContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the category service
        /// </summary>
        public CategoryService()
        {
            _dbContext = new DatabaseContext(ConfigurationService.ConnectionString);
        }

        /// <summary>
        /// Gets all product categories with their product counts
        /// </summary>
        /// <returns>A list of product categories ordered by name</returns>
        public async Task<List<Category>> GetProductCategoriesAsync()
        {
            try
            {
                Console.WriteLine("[CategoryService] Starting GetProductCategoriesAsync...");

                // Query categories of type "Product" and include product counts
                var categories = await _dbContext.Categories
                    .Where(c => c.Type == "Product")
                    .Select(c => new Category
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        Description = c.Description,
                        Type = c.Type,
                        ProductCount = _dbContext.Products.Count(p => p.CategoryId == c.CategoryId && p.IsActive)
                    })
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                Console.WriteLine($"[CategoryService] Found {categories.Count} product categories");

                // Log each category for debugging
                foreach (var category in categories)
                {
                    Console.WriteLine($"[CategoryService] Category: {category.Name} (ID: {category.CategoryId}, Products: {category.ProductCount})");
                }

                return categories;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoryService] Error in GetProductCategoriesAsync: {ex.Message}");
                Console.WriteLine($"[CategoryService] Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[CategoryService] Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"[CategoryService] Stack trace: {ex.StackTrace}");
                return new List<Category>();
            }
        }

        /// <summary>
        /// Gets a specific category by its ID
        /// </summary>
        /// <param name="categoryId">The category ID to search for</param>
        /// <returns>The category if found, otherwise null</returns>
        public async Task<Category> GetCategoryByIdAsync(int categoryId)
        {
            try
            {
                Console.WriteLine($"[CategoryService] Getting category by ID: {categoryId}");

                var category = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                if (category != null)
                {
                    // Calculate product count for this category
                    category.ProductCount = await _dbContext.Products
                        .CountAsync(p => p.CategoryId == categoryId && p.IsActive);

                    Console.WriteLine($"[CategoryService] Found category: {category.Name} with {category.ProductCount} products");
                }
                else
                {
                    Console.WriteLine($"[CategoryService] Category with ID {categoryId} not found");
                }

                return category;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoryService] Error in GetCategoryByIdAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[CategoryService] Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// Checks if a category exists in the database
        /// </summary>
        /// <param name="categoryId">The category ID to check</param>
        /// <returns>True if the category exists, otherwise false</returns>
        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            try
            {
                Console.WriteLine($"[CategoryService] Checking if category exists: {categoryId}");

                bool exists = await _dbContext.Categories
                    .AnyAsync(c => c.CategoryId == categoryId);

                Console.WriteLine($"[CategoryService] Category {categoryId} exists: {exists}");

                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoryService] Error in CategoryExistsAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[CategoryService] Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Gets categories with their product counts for display purposes
        /// Includes an "All Categories" option at the beginning of the list
        /// </summary>
        /// <returns>A list of categories including an "All" option</returns>
        public async Task<List<Category>> GetCategoriesForFilterAsync()
        {
            try
            {
                Console.WriteLine("[CategoryService] Getting categories for filter dropdown...");

                var categories = await GetProductCategoriesAsync();

                // Create "All Categories" option
                var allCategoriesOption = new Category
                {
                    CategoryId = 0, // Special ID for "All"
                    Name = "All Categories",
                    Description = "Show products from all categories",
                    Type = "Product",
                    ProductCount = categories.Sum(c => c.ProductCount)
                };

                Console.WriteLine($"[CategoryService] Created 'All Categories' option with {allCategoriesOption.ProductCount} total products");

                // Insert "All" option at the beginning
                var result = new List<Category> { allCategoriesOption };
                result.AddRange(categories);

                Console.WriteLine($"[CategoryService] Returning {result.Count} categories for filter (including 'All' option)");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoryService] Error in GetCategoriesForFilterAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[CategoryService] Inner exception: {ex.InnerException.Message}");
                }

                // Return at least the "All" option even if there's an error
                return new List<Category>
                {
                    new Category
                    {
                        CategoryId = 0,
                        Name = "All Categories",
                        Description = "Show all products",
                        Type = "Product",
                        ProductCount = 0
                    }
                };
            }
        }

        /// <summary>
        /// Gets the total count of active products across all categories
        /// </summary>
        /// <returns>Total number of active products</returns>
        public async Task<int> GetTotalProductCountAsync()
        {
            try
            {
                Console.WriteLine("[CategoryService] Getting total product count...");

                int totalCount = await _dbContext.Products
                    .CountAsync(p => p.IsActive);

                Console.WriteLine($"[CategoryService] Total active products: {totalCount}");

                return totalCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CategoryService] Error in GetTotalProductCountAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[CategoryService] Inner exception: {ex.InnerException.Message}");
                }
                return 0;
            }
        }
    }
}