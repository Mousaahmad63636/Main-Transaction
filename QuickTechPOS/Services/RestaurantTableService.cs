// File: QuickTechPOS/Services/RestaurantTableService.cs

using Microsoft.EntityFrameworkCore;
using QuickTechPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickTechPOS.Services
{
    /// <summary>
    /// Provides comprehensive data access and business logic for restaurant table management
    /// Implements enterprise-grade CRUD operations with optimized query patterns and caching
    /// </summary>
    public class RestaurantTableService : IDisposable
    {
        private readonly DatabaseContext _dbContext;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the RestaurantTableService
        /// </summary>
        public RestaurantTableService()
        {
            _dbContext = new DatabaseContext(ConfigurationService.ConnectionString);
        }

        /// <summary>
        /// Retrieves all active restaurant tables with optimized query performance
        /// </summary>
        /// <returns>Collection of active restaurant tables ordered by table number</returns>
        public async Task<List<RestaurantTable>> GetAllTablesAsync()
        {
            try
            {
                Console.WriteLine("[RestaurantTableService] Retrieving all restaurant tables...");

                var tables = await _dbContext.Set<RestaurantTable>()
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.TableNumber)
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[RestaurantTableService] Retrieved {tables.Count} active tables from database");
                return tables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error retrieving tables: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[RestaurantTableService] Inner exception: {ex.InnerException.Message}");
                }
                throw new Exception($"Failed to retrieve restaurant tables: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves tables filtered by status with high-performance querying
        /// </summary>
        /// <param name="status">Status filter (Available, Occupied, Reserved, Out of Service)</param>
        /// <returns>Collection of tables matching the specified status</returns>
        public async Task<List<RestaurantTable>> GetTablesByStatusAsync(string status)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Retrieving tables with status: {status}");

                if (string.IsNullOrWhiteSpace(status))
                {
                    return await GetAllTablesAsync();
                }

                var tables = await _dbContext.Set<RestaurantTable>()
                    .Where(t => t.IsActive &&
                               EF.Functions.Like(t.Status, status))
                    .OrderBy(t => t.TableNumber)
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[RestaurantTableService] Retrieved {tables.Count} tables with status '{status}'");
                return tables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error retrieving tables by status: {ex.Message}");
                throw new Exception($"Failed to retrieve tables by status '{status}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves only available tables for quick table selection
        /// </summary>
        /// <returns>Collection of available tables</returns>
        public async Task<List<RestaurantTable>> GetAvailableTablesAsync()
        {
            try
            {
                Console.WriteLine("[RestaurantTableService] Retrieving available tables...");

                var availableTables = await _dbContext.Set<RestaurantTable>()
                    .Where(t => t.IsActive && t.Status == "Available")
                    .OrderBy(t => t.TableNumber)
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[RestaurantTableService] Found {availableTables.Count} available tables");
                return availableTables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error retrieving available tables: {ex.Message}");
                throw new Exception($"Failed to retrieve available tables: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves a specific table by its unique identifier
        /// </summary>
        /// <param name="tableId">Unique table identifier</param>
        /// <returns>RestaurantTable if found, null otherwise</returns>
        public async Task<RestaurantTable> GetTableByIdAsync(int tableId)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Retrieving table with ID: {tableId}");

                if (tableId <= 0)
                {
                    Console.WriteLine("[RestaurantTableService] Invalid table ID provided");
                    return null;
                }

                var table = await _dbContext.Set<RestaurantTable>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tableId);

                if (table != null)
                {
                    Console.WriteLine($"[RestaurantTableService] Found table: {table.DisplayName} ({table.Status})");
                }
                else
                {
                    Console.WriteLine($"[RestaurantTableService] Table with ID {tableId} not found");
                }

                return table;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error retrieving table by ID: {ex.Message}");
                throw new Exception($"Failed to retrieve table with ID {tableId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves a table by its table number with collision detection
        /// </summary>
        /// <param name="tableNumber">Table number to search for</param>
        /// <returns>RestaurantTable if found, null otherwise</returns>
        public async Task<RestaurantTable> GetTableByNumberAsync(int tableNumber)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Retrieving table with number: {tableNumber}");

                if (tableNumber <= 0)
                {
                    Console.WriteLine("[RestaurantTableService] Invalid table number provided");
                    return null;
                }

                var table = await _dbContext.Set<RestaurantTable>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TableNumber == tableNumber && t.IsActive);

                if (table != null)
                {
                    Console.WriteLine($"[RestaurantTableService] Found table number {tableNumber}: ID={table.Id} ({table.Status})");
                }
                else
                {
                    Console.WriteLine($"[RestaurantTableService] Table number {tableNumber} not found or inactive");
                }

                return table;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error retrieving table by number: {ex.Message}");
                throw new Exception($"Failed to retrieve table number {tableNumber}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates the status of a restaurant table with transaction safety
        /// </summary>
        /// <param name="tableId">Unique table identifier</param>
        /// <param name="newStatus">New status to set</param>
        /// <returns>True if update successful, false otherwise</returns>
        public async Task<bool> UpdateTableStatusAsync(int tableId, string newStatus)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Updating table {tableId} status to: {newStatus}");

                if (tableId <= 0 || string.IsNullOrWhiteSpace(newStatus))
                {
                    Console.WriteLine("[RestaurantTableService] Invalid parameters for status update");
                    return false;
                }

                if (!RestaurantTable.IsValidStatus(newStatus))
                {
                    Console.WriteLine($"[RestaurantTableService] Invalid status provided: {newStatus}");
                    return false;
                }

                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var table = await _dbContext.Set<RestaurantTable>()
                        .FirstOrDefaultAsync(t => t.Id == tableId);

                    if (table == null)
                    {
                        Console.WriteLine($"[RestaurantTableService] Table with ID {tableId} not found for update");
                        return false;
                    }

                    var oldStatus = table.Status;
                    table.UpdateStatus(newStatus);

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Console.WriteLine($"[RestaurantTableService] Successfully updated table {tableId} status from '{oldStatus}' to '{newStatus}'");
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error updating table status: {ex.Message}");
                throw new Exception($"Failed to update table {tableId} status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a new restaurant table with validation and duplicate checking
        /// </summary>
        /// <param name="tableNumber">Table number for the new table</param>
        /// <param name="description">Optional description</param>
        /// <returns>Created RestaurantTable if successful, null otherwise</returns>
        public async Task<RestaurantTable> CreateTableAsync(int tableNumber, string description = "")
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Creating new table with number: {tableNumber}");

                if (tableNumber <= 0)
                {
                    Console.WriteLine("[RestaurantTableService] Invalid table number for creation");
                    return null;
                }

                // Check for existing table with same number
                var existingTable = await GetTableByNumberAsync(tableNumber);
                if (existingTable != null)
                {
                    Console.WriteLine($"[RestaurantTableService] Table number {tableNumber} already exists");
                    throw new InvalidOperationException($"Table number {tableNumber} already exists");
                }

                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var newTable = new RestaurantTable
                    {
                        TableNumber = tableNumber,
                        Status = "Available",
                        Description = description ?? string.Empty,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };

                    _dbContext.Set<RestaurantTable>().Add(newTable);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Console.WriteLine($"[RestaurantTableService] Successfully created table {tableNumber} with ID: {newTable.Id}");
                    return newTable;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error creating table: {ex.Message}");
                throw new Exception($"Failed to create table {tableNumber}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Soft deletes a table by marking it as inactive
        /// </summary>
        /// <param name="tableId">Unique table identifier</param>
        /// <returns>True if deactivation successful, false otherwise</returns>
        public async Task<bool> DeactivateTableAsync(int tableId)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Deactivating table with ID: {tableId}");

                if (tableId <= 0)
                {
                    Console.WriteLine("[RestaurantTableService] Invalid table ID for deactivation");
                    return false;
                }

                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var table = await _dbContext.Set<RestaurantTable>()
                        .FirstOrDefaultAsync(t => t.Id == tableId);

                    if (table == null)
                    {
                        Console.WriteLine($"[RestaurantTableService] Table with ID {tableId} not found for deactivation");
                        return false;
                    }

                    table.IsActive = false;
                    table.UpdatedAt = DateTime.Now;

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Console.WriteLine($"[RestaurantTableService] Successfully deactivated table {tableId}");
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error deactivating table: {ex.Message}");
                throw new Exception($"Failed to deactivate table {tableId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets comprehensive table statistics for reporting and analytics
        /// </summary>
        /// <returns>Dictionary containing table statistics</returns>
        public async Task<Dictionary<string, int>> GetTableStatisticsAsync()
        {
            try
            {
                Console.WriteLine("[RestaurantTableService] Generating table statistics...");

                var stats = new Dictionary<string, int>();

                var allTables = await _dbContext.Set<RestaurantTable>()
                    .Where(t => t.IsActive)
                    .AsNoTracking()
                    .ToListAsync();

                stats["TotalTables"] = allTables.Count;
                stats["AvailableTables"] = allTables.Count(t => t.Status == "Available");
                stats["OccupiedTables"] = allTables.Count(t => t.Status == "Occupied");
                stats["ReservedTables"] = allTables.Count(t => t.Status == "Reserved");
                stats["OutOfServiceTables"] = allTables.Count(t => t.Status == "Out of Service");

                Console.WriteLine($"[RestaurantTableService] Generated statistics: Total={stats["TotalTables"]}, " +
                                 $"Available={stats["AvailableTables"]}, Occupied={stats["OccupiedTables"]}, " +
                                 $"Reserved={stats["ReservedTables"]}, OutOfService={stats["OutOfServiceTables"]}");

                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error generating statistics: {ex.Message}");
                throw new Exception($"Failed to generate table statistics: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates database connectivity and table structure
        /// </summary>
        /// <returns>True if database connection and table structure are valid</returns>
        public async Task<bool> ValidateDatabaseConnectionAsync()
        {
            try
            {
                Console.WriteLine("[RestaurantTableService] Validating database connection...");

                // Test basic connectivity
                await _dbContext.Database.OpenConnectionAsync();
                await _dbContext.Database.CloseConnectionAsync();

                // Test table existence and structure
                var tableCount = await _dbContext.Set<RestaurantTable>().CountAsync();

                Console.WriteLine($"[RestaurantTableService] Database validation successful. Table count: {tableCount}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Database validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Releases database resources and performs cleanup
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose pattern implementation
        /// </summary>
        /// <param name="disposing">Whether disposing is being called explicitly</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    _dbContext?.Dispose();
                    Console.WriteLine("[RestaurantTableService] Database context disposed successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RestaurantTableService] Error during disposal: {ex.Message}");
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer to ensure proper resource cleanup
        /// </summary>
        ~RestaurantTableService()
        {
            Dispose(false);
        }
    }
}