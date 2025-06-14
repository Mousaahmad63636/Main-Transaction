using Microsoft.EntityFrameworkCore;
using QuickTechPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickTechPOS.Services
{
    public class RestaurantTableService : IDisposable
    {
        private readonly DatabaseContext _dbContext;
        private bool _disposed = false;

        public RestaurantTableService()
        {
            _dbContext = new DatabaseContext(ConfigurationService.ConnectionString);
        }

        public async Task<List<RestaurantTable>> GetAllTablesAsync()
        {
            try
            {
                Console.WriteLine("[RestaurantTableService] Retrieving all restaurant tables with enhanced status tracking...");

                var tables = await _dbContext.Set<RestaurantTable>()
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.TableNumber)
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[RestaurantTableService] Retrieved {tables.Count} active tables from database with current status");
                return tables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error retrieving enhanced tables: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[RestaurantTableService] Inner exception: {ex.InnerException.Message}");
                }
                throw new Exception($"Failed to retrieve restaurant tables: {ex.Message}", ex);
            }
        }

        public async Task<List<RestaurantTable>> GetTablesByStatusAsync(string status)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Retrieving enhanced tables with status: {status}");

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

                Console.WriteLine($"[RestaurantTableService] Retrieved {tables.Count} enhanced tables with status '{status}'");
                return tables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error retrieving enhanced tables by status: {ex.Message}");
                throw new Exception($"Failed to retrieve tables by status '{status}': {ex.Message}", ex);
            }
        }

        public async Task<List<RestaurantTable>> GetAvailableTablesAsync()
        {
            try
            {
                Console.WriteLine("[RestaurantTableService] Retrieving enhanced available tables...");

                var availableTables = await _dbContext.Set<RestaurantTable>()
                    .Where(t => t.IsActive && t.Status == "Available")
                    .OrderBy(t => t.TableNumber)
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[RestaurantTableService] Found {availableTables.Count} enhanced available tables");
                return availableTables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error retrieving enhanced available tables: {ex.Message}");
                throw new Exception($"Failed to retrieve available tables: {ex.Message}", ex);
            }
        }

        public async Task<RestaurantTable> GetTableByIdAsync(int tableId)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Retrieving enhanced table with ID: {tableId}");

                if (tableId <= 0)
                {
                    Console.WriteLine("[RestaurantTableService] Invalid enhanced table ID provided");
                    return null;
                }

                var table = await _dbContext.Set<RestaurantTable>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tableId);

                if (table != null)
                {
                    Console.WriteLine($"[RestaurantTableService] Found enhanced table: {table.DisplayName} ({table.Status})");
                }
                else
                {
                    Console.WriteLine($"[RestaurantTableService] Enhanced table with ID {tableId} not found");
                }

                return table;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error retrieving enhanced table by ID: {ex.Message}");
                throw new Exception($"Failed to retrieve table with ID {tableId}: {ex.Message}", ex);
            }
        }

        public async Task<RestaurantTable> GetTableByNumberAsync(int tableNumber)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Retrieving enhanced table with number: {tableNumber}");

                if (tableNumber <= 0)
                {
                    Console.WriteLine("[RestaurantTableService] Invalid enhanced table number provided");
                    return null;
                }

                var table = await _dbContext.Set<RestaurantTable>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TableNumber == tableNumber && t.IsActive);

                if (table != null)
                {
                    Console.WriteLine($"[RestaurantTableService] Found enhanced table number {tableNumber}: ID={table.Id} ({table.Status})");
                }
                else
                {
                    Console.WriteLine($"[RestaurantTableService] Enhanced table number {tableNumber} not found or inactive");
                }

                return table;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error retrieving enhanced table by number: {ex.Message}");
                throw new Exception($"Failed to retrieve table number {tableNumber}: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateTableStatusAsync(int tableId, string newStatus)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Updating enhanced table {tableId} status to: {newStatus}");

                if (tableId <= 0 || string.IsNullOrWhiteSpace(newStatus))
                {
                    Console.WriteLine("[RestaurantTableService] Invalid enhanced parameters for status update");
                    return false;
                }

                if (!RestaurantTable.IsValidStatus(newStatus))
                {
                    Console.WriteLine($"[RestaurantTableService] Invalid enhanced status provided: {newStatus}");
                    return false;
                }

                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var table = await _dbContext.Set<RestaurantTable>()
                        .FirstOrDefaultAsync(t => t.Id == tableId);

                    if (table == null)
                    {
                        Console.WriteLine($"[RestaurantTableService] Enhanced table with ID {tableId} not found for update");
                        return false;
                    }

                    var oldStatus = table.Status;
                    table.UpdateStatus(newStatus);

                    var rowsAffected = await _dbContext.SaveChangesAsync();

                    if (rowsAffected > 0)
                    {
                        await transaction.CommitAsync();
                        Console.WriteLine($"[RestaurantTableService] Successfully updated enhanced table {tableId} status from '{oldStatus}' to '{newStatus}' (rows affected: {rowsAffected})");
                        return true;
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine($"[RestaurantTableService] No rows affected when updating enhanced table {tableId} status");
                        return false;
                    }
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error updating enhanced table status: {ex.Message}");
                throw new Exception($"Failed to update table {tableId} status: {ex.Message}", ex);
            }
        }

        public async Task<bool> BulkUpdateTableStatusAsync(Dictionary<int, string> tableStatusUpdates)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Starting enhanced bulk status update for {tableStatusUpdates.Count} tables");

                if (tableStatusUpdates == null || tableStatusUpdates.Count == 0)
                {
                    Console.WriteLine("[RestaurantTableService] No enhanced table status updates provided");
                    return false;
                }

                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    int totalUpdated = 0;

                    foreach (var update in tableStatusUpdates)
                    {
                        var tableId = update.Key;
                        var newStatus = update.Value;

                        if (tableId <= 0 || string.IsNullOrWhiteSpace(newStatus) || !RestaurantTable.IsValidStatus(newStatus))
                        {
                            Console.WriteLine($"[RestaurantTableService] Skipping invalid enhanced update: TableId={tableId}, Status='{newStatus}'");
                            continue;
                        }

                        var table = await _dbContext.Set<RestaurantTable>()
                            .FirstOrDefaultAsync(t => t.Id == tableId);

                        if (table != null && table.Status != newStatus)
                        {
                            var oldStatus = table.Status;
                            table.UpdateStatus(newStatus);
                            totalUpdated++;

                            Console.WriteLine($"[RestaurantTableService] Enhanced bulk update: Table {tableId} ({table.DisplayName}) from '{oldStatus}' to '{newStatus}'");
                        }
                    }

                    if (totalUpdated > 0)
                    {
                        var rowsAffected = await _dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        Console.WriteLine($"[RestaurantTableService] Enhanced bulk update completed: {totalUpdated} tables updated, {rowsAffected} rows affected");
                        return true;
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("[RestaurantTableService] Enhanced bulk update: No valid updates to process");
                        return false;
                    }
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error in enhanced bulk table status update: {ex.Message}");
                throw new Exception($"Failed to bulk update table statuses: {ex.Message}", ex);
            }
        }

        public async Task<bool> SetTableOccupiedAsync(int tableId)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Setting enhanced table {tableId} to Occupied (red status)");
                return await UpdateTableStatusAsync(tableId, "Occupied");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error setting enhanced table {tableId} to Occupied: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetTableAvailableAsync(int tableId)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Setting enhanced table {tableId} to Available (green status)");
                return await UpdateTableStatusAsync(tableId, "Available");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error setting enhanced table {tableId} to Available: {ex.Message}");
                return false;
            }
        }

        public async Task<RestaurantTable> CreateTableAsync(int tableNumber, string description = "")
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Creating enhanced new table with number: {tableNumber}");

                if (tableNumber <= 0)
                {
                    Console.WriteLine("[RestaurantTableService] Invalid enhanced table number for creation");
                    return null;
                }

                var existingTable = await GetTableByNumberAsync(tableNumber);
                if (existingTable != null)
                {
                    Console.WriteLine($"[RestaurantTableService] Enhanced table number {tableNumber} already exists");
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

                    Console.WriteLine($"[RestaurantTableService] Successfully created enhanced table {tableNumber} with ID: {newTable.Id}");
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
                Console.WriteLine($"[RestaurantTableService] Error creating enhanced table: {ex.Message}");
                throw new Exception($"Failed to create table {tableNumber}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeactivateTableAsync(int tableId)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Deactivating enhanced table with ID: {tableId}");

                if (tableId <= 0)
                {
                    Console.WriteLine("[RestaurantTableService] Invalid enhanced table ID for deactivation");
                    return false;
                }

                using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    var table = await _dbContext.Set<RestaurantTable>()
                        .FirstOrDefaultAsync(t => t.Id == tableId);

                    if (table == null)
                    {
                        Console.WriteLine($"[RestaurantTableService] Enhanced table with ID {tableId} not found for deactivation");
                        return false;
                    }

                    table.IsActive = false;
                    table.UpdatedAt = DateTime.Now;

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Console.WriteLine($"[RestaurantTableService] Successfully deactivated enhanced table {tableId}");
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
                Console.WriteLine($"[RestaurantTableService] Error deactivating enhanced table: {ex.Message}");
                throw new Exception($"Failed to deactivate table {tableId}: {ex.Message}", ex);
            }
        }

        public async Task<Dictionary<string, int>> GetTableStatisticsAsync()
        {
            try
            {
                Console.WriteLine("[RestaurantTableService] Generating enhanced table statistics...");

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

                Console.WriteLine($"[RestaurantTableService] Generated enhanced statistics: Total={stats["TotalTables"]}, " +
                                 $"Available={stats["AvailableTables"]}, Occupied={stats["OccupiedTables"]}, " +
                                 $"Reserved={stats["ReservedTables"]}, OutOfService={stats["OutOfServiceTables"]}");

                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error generating enhanced statistics: {ex.Message}");
                throw new Exception($"Failed to generate table statistics: {ex.Message}", ex);
            }
        }

        public async Task<bool> ValidateDatabaseConnectionAsync()
        {
            try
            {
                Console.WriteLine("[RestaurantTableService] Validating enhanced database connection...");

                await _dbContext.Database.OpenConnectionAsync();
                await _dbContext.Database.CloseConnectionAsync();

                var tableCount = await _dbContext.Set<RestaurantTable>().CountAsync();

                Console.WriteLine($"[RestaurantTableService] Enhanced database validation successful. Table count: {tableCount}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Enhanced database validation failed: {ex.Message}");
                return false;
            }
        }

        public async Task<List<RestaurantTable>> GetTablesWithItemsAsync()
        {
            try
            {
                Console.WriteLine("[RestaurantTableService] Retrieving enhanced tables that should be marked as Occupied (red)");

                var occupiedTables = await _dbContext.Set<RestaurantTable>()
                    .Where(t => t.IsActive && t.Status == "Occupied")
                    .OrderBy(t => t.TableNumber)
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[RestaurantTableService] Found {occupiedTables.Count} enhanced tables with Occupied status (red)");
                return occupiedTables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error retrieving enhanced tables with items: {ex.Message}");
                throw new Exception($"Failed to retrieve tables with items: {ex.Message}", ex);
            }
        }

        public async Task<List<RestaurantTable>> GetEmptyTablesAsync()
        {
            try
            {
                Console.WriteLine("[RestaurantTableService] Retrieving enhanced tables that should be marked as Available (green)");

                var availableTables = await _dbContext.Set<RestaurantTable>()
                    .Where(t => t.IsActive && t.Status == "Available")
                    .OrderBy(t => t.TableNumber)
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"[RestaurantTableService] Found {availableTables.Count} enhanced tables with Available status (green)");
                return availableTables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error retrieving enhanced empty tables: {ex.Message}");
                throw new Exception($"Failed to retrieve empty tables: {ex.Message}", ex);
            }
        }

        public async Task<bool> SynchronizeTableStatusesAsync(Dictionary<int, bool> tableItemStatus)
        {
            try
            {
                Console.WriteLine($"[RestaurantTableService] Synchronizing enhanced table statuses for {tableItemStatus.Count} tables");

                var statusUpdates = new Dictionary<int, string>();

                foreach (var kvp in tableItemStatus)
                {
                    int tableId = kvp.Key;
                    bool hasItems = kvp.Value;

                    string targetStatus = hasItems ? "Occupied" : "Available";
                    statusUpdates[tableId] = targetStatus;

                    Console.WriteLine($"[RestaurantTableService] Enhanced sync: Table {tableId} -> {targetStatus} ({(hasItems ? "red" : "green")})");
                }

                bool success = await BulkUpdateTableStatusAsync(statusUpdates);

                Console.WriteLine($"[RestaurantTableService] Enhanced table status synchronization {(success ? "completed successfully" : "failed")}");
                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestaurantTableService] Error in enhanced table status synchronization: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    _dbContext?.Dispose();
                    Console.WriteLine("[RestaurantTableService] Enhanced database context disposed successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RestaurantTableService] Error during enhanced disposal: {ex.Message}");
                }
                _disposed = true;
            }
        }

        ~RestaurantTableService()
        {
            Dispose(false);
        }
    }
}