// File: QuickTechPOS/Services/ProductService.cs

using Microsoft.EntityFrameworkCore;
using QuickTechPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickTechPOS.Services
{
    /// <summary>
    /// Provides operations for managing products
    /// </summary>
    public class ProductService
    {
        private readonly DatabaseContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the product service
        /// </summary>
        public ProductService()
        {
            _dbContext = new DatabaseContext(ConfigurationService.ConnectionString);
        }

        /// <summary>
        /// Searches for products by category
        /// </summary>
        /// <param name="categoryId">The category ID to filter by (0 for all categories)</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>A list of matching products</returns>
        public async Task<List<Product>> SearchByCategoryAsync(int categoryId, int maxResults = 50)
        {
            try
            {
                Console.WriteLine($"[ProductService] Starting SearchByCategoryAsync with categoryId: {categoryId}, maxResults: {maxResults}");

                if (categoryId == 0)
                {
                    // Return all active products when categoryId is 0 (All Categories)
                    Console.WriteLine("[ProductService] CategoryId is 0, fetching all active products");

                    var allProducts = await _dbContext.Products
                        .Where(p => p.IsActive)
                        .OrderBy(p => p.Name)
                        .Take(maxResults)
                        .ToListAsync();

                    Console.WriteLine($"[ProductService] Found {allProducts.Count} active products for 'All Categories'");

                    // Log first few products for debugging
                    for (int i = 0; i < Math.Min(5, allProducts.Count); i++)
                    {
                        var product = allProducts[i];
                        Console.WriteLine($"[ProductService] Product {i + 1}: {product.Name} (ID: {product.ProductId}, Category: {product.CategoryId}, Stock: {product.CurrentStock})");
                    }

                    return allProducts;
                }
                else
                {
                    // Filter by specific category
                    Console.WriteLine($"[ProductService] Filtering by specific category: {categoryId}");

                    var categoryProducts = await _dbContext.Products
                        .Where(p => p.IsActive && p.CategoryId == categoryId)
                        .OrderBy(p => p.Name)
                        .Take(maxResults)
                        .ToListAsync();

                    Console.WriteLine($"[ProductService] Found {categoryProducts.Count} products in category {categoryId}");

                    // Log products in this category for debugging
                    foreach (var product in categoryProducts)
                    {
                        Console.WriteLine($"[ProductService] Category Product: {product.Name} (ID: {product.ProductId}, Stock: {product.CurrentStock})");
                    }

                    return categoryProducts;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductService] Error in SearchByCategoryAsync: {ex.Message}");
                Console.WriteLine($"[ProductService] Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ProductService] Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"[ProductService] Stack trace: {ex.StackTrace}");
                return new List<Product>();
            }
        }

        /// <summary>
        /// Searches for products by name (legacy method - kept for backward compatibility)
        /// </summary>
        /// <param name="query">The search query</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>A list of matching products</returns>
        public async Task<List<Product>> SearchByNameAsync(string query, int maxResults = 10)
        {
            try
            {
                Console.WriteLine($"[ProductService] Starting SearchByNameAsync with query: '{query}', maxResults: {maxResults}");

                if (string.IsNullOrWhiteSpace(query))
                {
                    // Return all products if query is empty
                    Console.WriteLine("[ProductService] Query is empty, returning all active products");

                    var allProducts = await _dbContext.Products
                        .Where(p => p.IsActive)
                        .OrderBy(p => p.Name)
                        .Take(maxResults)
                        .ToListAsync();

                    Console.WriteLine($"[ProductService] Found {allProducts.Count} active products for empty query");
                    return allProducts;
                }

                // Safe search using EF.Functions.Like with null protection
                Console.WriteLine($"[ProductService] Performing name search for: '{query}'");

                var products = await _dbContext.Products
                    .Where(p => p.IsActive && p.Name != null && p.Name.Contains(query))
                    .OrderBy(p => p.Name)
                    .Take(maxResults)
                    .ToListAsync();

                Console.WriteLine($"[ProductService] Found {products.Count} products matching '{query}'");

                // Log found products for debugging
                foreach (var product in products)
                {
                    Console.WriteLine($"[ProductService] Found product: {product.Name} (ID: {product.ProductId}, Category: {product.CategoryId})");
                }

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductService] Error in SearchByNameAsync: {ex.Message}");
                Console.WriteLine($"[ProductService] Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ProductService] Inner exception: {ex.InnerException.Message}");
                }
                return new List<Product>();
            }
        }

        /// <summary>
        /// Searches for products by category and name combined
        /// </summary>
        /// <param name="categoryId">The category ID to filter by (0 for all categories)</param>
        /// <param name="nameQuery">Optional name search query</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>A list of matching products</returns>
        public async Task<List<Product>> SearchByCategoryAndNameAsync(int categoryId, string nameQuery = "", int maxResults = 50)
        {
            try
            {
                Console.WriteLine($"[ProductService] Starting SearchByCategoryAndNameAsync - CategoryId: {categoryId}, NameQuery: '{nameQuery}', MaxResults: {maxResults}");

                var query = _dbContext.Products.Where(p => p.IsActive);

                // Apply category filter if not "All Categories"
                if (categoryId > 0)
                {
                    Console.WriteLine($"[ProductService] Applying category filter for categoryId: {categoryId}");
                    query = query.Where(p => p.CategoryId == categoryId);
                }

                // Apply name filter if provided
                if (!string.IsNullOrWhiteSpace(nameQuery))
                {
                    Console.WriteLine($"[ProductService] Applying name filter for: '{nameQuery}'");
                    query = query.Where(p => p.Name != null && p.Name.Contains(nameQuery));
                }

                var products = await query
                    .OrderBy(p => p.Name)
                    .Take(maxResults)
                    .ToListAsync();

                Console.WriteLine($"[ProductService] SearchByCategoryAndNameAsync found {products.Count} products");

                // Log first few results for debugging
                for (int i = 0; i < Math.Min(3, products.Count); i++)
                {
                    var product = products[i];
                    Console.WriteLine($"[ProductService] Result {i + 1}: {product.Name} (ID: {product.ProductId}, Category: {product.CategoryId})");
                }

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductService] Error in SearchByCategoryAndNameAsync: {ex.Message}");
                Console.WriteLine($"[ProductService] Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ProductService] Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"[ProductService] Stack trace: {ex.StackTrace}");
                return new List<Product>();
            }
        }

        /// <summary>
        /// Gets products by category with enhanced debugging and error handling
        /// </summary>
        /// <param name="categoryId">The category ID to filter by</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>A list of products in the specified category</returns>
        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int maxResults = 100)
        {
            try
            {
                Console.WriteLine($"[ProductService] Getting products for categoryId: {categoryId}, maxResults: {maxResults}");

                var products = await _dbContext.Products
                    .Where(p => p.IsActive && p.CategoryId == categoryId)
                    .OrderBy(p => p.Name)
                    .Take(maxResults)
                    .ToListAsync();

                Console.WriteLine($"[ProductService] GetProductsByCategoryAsync returned {products.Count} products for category {categoryId}");

                // Log product details for debugging
                foreach (var product in products)
                {
                    Console.WriteLine($"[ProductService] Product in category {categoryId}: {product.Name} (Stock: {product.CurrentStock}, Price: ${product.SalePrice:F2})");
                }

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductService] Error in GetProductsByCategoryAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ProductService] Inner exception: {ex.InnerException.Message}");
                }
                return new List<Product>();
            }
        }

        public async Task<ProductSearchResult> FindByAnyBarcodeAsync(string barcode)
        {
            try
            {
                Console.WriteLine($"[ProductService] Starting FindByAnyBarcodeAsync with barcode: '{barcode}'");

                if (string.IsNullOrWhiteSpace(barcode))
                {
                    Console.WriteLine("[ProductService] Barcode is null or empty, returning null");
                    return null;
                }

                // First check if it matches a box barcode
                Console.WriteLine("[ProductService] Checking for box barcode match...");
                var boxProduct = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.IsActive && p.BoxBarcode == barcode);

                if (boxProduct != null)
                {
                    Console.WriteLine($"[ProductService] Found box barcode match: {boxProduct.Name} (ID: {boxProduct.ProductId})");
                    return new ProductSearchResult
                    {
                        Product = boxProduct,
                        IsBoxBarcode = true
                    };
                }

                // Then check if it matches a regular barcode
                Console.WriteLine("[ProductService] Checking for regular barcode match...");
                var regularProduct = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.IsActive && p.Barcode == barcode);

                if (regularProduct != null)
                {
                    Console.WriteLine($"[ProductService] Found regular barcode match: {regularProduct.Name} (ID: {regularProduct.ProductId})");
                    return new ProductSearchResult
                    {
                        Product = regularProduct,
                        IsBoxBarcode = false
                    };
                }

                Console.WriteLine($"[ProductService] No product found for barcode: '{barcode}'");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductService] Error in FindByAnyBarcodeAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ProductService] Inner exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        public class ProductSearchResult
        {
            public Product Product { get; set; }
            public bool IsBoxBarcode { get; set; }
        }
        /// <summary>
        /// Updates the box stock of a product
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <param name="boxQuantitySold">The quantity of boxes sold</param>
        /// <returns>True if the update was successful, otherwise false</returns>
        public async Task<bool> UpdateBoxStockAsync(int productId, decimal boxQuantitySold)
        {
            try
            {
                Console.WriteLine($"[ProductService] Updating box stock for productId: {productId}, boxQuantitySold: {boxQuantitySold}");

                var product = await _dbContext.Products.FindAsync(productId);
                if (product == null)
                {
                    Console.WriteLine($"[ProductService] Product with ID {productId} not found during box stock update");
                    return false;
                }

                // Convert decimal box quantity to int (round down)
                int wholeBoxesQuantity = (int)Math.Floor(boxQuantitySold);

                // Calculate items per box
                int itemsPerBox = product.ItemsPerBox > 0 ? product.ItemsPerBox : 1;

                // Calculate total individual items being sold
                decimal totalIndividualItems = boxQuantitySold * itemsPerBox;

                Console.WriteLine($"[ProductService] Box update calculations - WholeBoxes: {wholeBoxesQuantity}, ItemsPerBox: {itemsPerBox}, TotalItems: {totalIndividualItems}");

                // Update box inventory
                if (product.NumberOfBoxes < wholeBoxesQuantity)
                {
                    Console.WriteLine($"[ProductService] Warning: Insufficient box stock for product {productId}. Available: {product.NumberOfBoxes}, Requested: {wholeBoxesQuantity}");
                    product.NumberOfBoxes = 0;
                }
                else
                {
                    product.NumberOfBoxes -= wholeBoxesQuantity;
                }

                // IMPORTANT: Also update the individual items stock
                if (product.CurrentStock < totalIndividualItems)
                {
                    Console.WriteLine($"[ProductService] Warning: Insufficient item stock for product {productId}. Available: {product.CurrentStock}, Needed: {totalIndividualItems}");
                    product.CurrentStock = 0;
                }
                else
                {
                    product.CurrentStock -= totalIndividualItems;
                }

                product.UpdatedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                Console.WriteLine($"[ProductService] Successfully updated box stock for product #{productId}: Boxes={product.NumberOfBoxes}, Items={product.CurrentStock}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductService] Error in UpdateBoxStockAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ProductService] Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            try
            {
                Console.WriteLine($"[ProductService] Getting product by ID: {productId}");

                var product = await _dbContext.Products.FindAsync(productId);

                if (product != null)
                {
                    Console.WriteLine($"[ProductService] Found product: {product.Name} (Category: {product.CategoryId})");
                }
                else
                {
                    Console.WriteLine($"[ProductService] Product with ID {productId} not found");
                }

                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductService] Error retrieving product: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Updates the stock of a product
        /// </summary>
        /// <param name="productId">The product ID</param>
        /// <param name="quantitySold">The quantity sold</param>
        /// <returns>True if the update was successful, otherwise false</returns>
        public async Task<bool> UpdateStockAsync(int productId, decimal quantitySold)
        {
            try
            {
                Console.WriteLine($"[ProductService] Updating stock for productId: {productId}, quantitySold: {quantitySold}");

                var product = await _dbContext.Products.FindAsync(productId);
                if (product == null)
                {
                    Console.WriteLine($"[ProductService] Product with ID {productId} not found during stock update");
                    return false;
                }

                Console.WriteLine($"[ProductService] Current stock before update: {product.CurrentStock}");

                // Check if we have enough stock before updating
                if (product.CurrentStock < quantitySold)
                {
                    Console.WriteLine($"[ProductService] Warning: Insufficient stock for product {productId}. Available: {product.CurrentStock}, Requested: {quantitySold}");
                    // Depending on your business logic, you might want to:
                    // 1. Allow negative stock (remove this check)
                    // 2. Set stock to 0 instead of going negative
                    // 3. Fail the update (return false)

                    // For now, we'll set it to 0 if it would go negative
                    product.CurrentStock = 0;
                }
                else
                {
                    product.CurrentStock -= quantitySold;
                }

                product.UpdatedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                Console.WriteLine($"[ProductService] Successfully updated stock for product #{productId}: New stock level = {product.CurrentStock}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductService] Error in UpdateStockAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ProductService] Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Checks if the database contains any product with the specified name
        /// </summary>
        /// <param name="productName">The product name to check</param>
        /// <returns>True if the product exists, otherwise false</returns>
        public async Task<bool> ProductExistsAsync(string productName)
        {
            try
            {
                Console.WriteLine($"[ProductService] Checking if product exists: '{productName}'");

                if (string.IsNullOrWhiteSpace(productName))
                {
                    Console.WriteLine("[ProductService] Product name is null or empty, returning false");
                    return false;
                }

                bool exists = await _dbContext.Products
                    .AnyAsync(p => p.IsActive && p.Name != null && p.Name.Contains(productName));

                Console.WriteLine($"[ProductService] Product exists check result: {exists}");

                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductService] Error in ProductExistsAsync: {ex.Message}");
                return false;
            }
        }
    }
}