using QuickTechPOS.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using QuickTechPOS.Helpers;
using System.Threading;

namespace QuickTechPOS.Services
{
    /// <summary>
    /// Enhanced table status synchronization service with immediate updates and thread safety
    /// </summary>
    public class TableStatusSynchronizationService : IDisposable
    {
        private readonly RestaurantTableService _tableService;
        private readonly ConcurrentDictionary<int, TableStatusInfo> _tableStatusCache;
        private readonly SemaphoreSlim _syncSemaphore;
        private readonly object _syncLock = new object();
        private bool _disposed = false;
        private readonly SemaphoreSlim _tableSwitchLock = new SemaphoreSlim(1, 1);
        private volatile bool _isLoadingTableState = false;

        public event EventHandler<TableStatusChangedEventArgs> TableStatusChanged;

        private class TableStatusInfo
        {
            public int TableId { get; set; }
            public string Status { get; set; }
            public int ItemCount { get; set; }
            public DateTime LastUpdated { get; set; }
            public volatile bool IsSyncing = false;
        }

        public class TableStatusChangedEventArgs : EventArgs
        {
            public int TableId { get; set; }
            public string OldStatus { get; set; }
            public string NewStatus { get; set; }
            public int ItemCount { get; set; }
            public DateTime ChangeTime { get; set; }
        }

        public TableStatusSynchronizationService()
        {
            Console.WriteLine("[TableStatusSynchronizationService] Initializing immediate synchronization service...");

            _tableService = new RestaurantTableService();
            _tableStatusCache = new ConcurrentDictionary<int, TableStatusInfo>();
            _syncSemaphore = new SemaphoreSlim(1, 1);

            Console.WriteLine("[TableStatusSynchronizationService] Immediate synchronization service initialized");
        }

        /// <summary>
        /// Updates table item count with immediate status synchronization
        /// </summary>
        public async Task UpdateTableItemCountAsync(int tableId, int itemCount)
        {
            try
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Immediate update for table {tableId}: {itemCount} items");

                var statusInfo = _tableStatusCache.GetOrAdd(tableId, id => new TableStatusInfo
                {
                    TableId = id,
                    Status = "Available",
                    ItemCount = 0,
                    LastUpdated = DateTime.Now
                });

                string expectedStatus = itemCount > 0 ? "Occupied" : "Available";
                bool statusChanged = statusInfo.Status != expectedStatus;
                bool itemCountChanged = statusInfo.ItemCount != itemCount;

                if (statusChanged || itemCountChanged)
                {
                    string oldStatus = statusInfo.Status;

                    // Update cache immediately
                    statusInfo.ItemCount = itemCount;
                    statusInfo.Status = expectedStatus;
                    statusInfo.LastUpdated = DateTime.Now;

                    Console.WriteLine($"[TableStatusSynchronizationService] Status change detected for table {tableId}: " +
                                     $"Status: '{oldStatus}' -> '{expectedStatus}', Items: {itemCount}");

                    // Immediate database synchronization
                    await SynchronizeTableStatusImmediately(tableId, expectedStatus);

                    // Raise event for UI updates
                    if (statusChanged)
                    {
                        OnTableStatusChanged(new TableStatusChangedEventArgs
                        {
                            TableId = tableId,
                            OldStatus = oldStatus,
                            NewStatus = expectedStatus,
                            ItemCount = itemCount,
                            ChangeTime = DateTime.Now
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error updating table item count: {ex.Message}");
            }
        }

        /// <summary>
        /// Synchronizes table status to database immediately with thread safety
        /// </summary>
        private async Task SynchronizeTableStatusImmediately(int tableId, string status)
        {
            if (!await _syncSemaphore.WaitAsync(5000)) // 5 second timeout
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Timeout waiting for sync semaphore for table {tableId}");
                return;
            }

            try
            {
                var statusInfo = _tableStatusCache.GetValueOrDefault(tableId);
                if (statusInfo == null || statusInfo.IsSyncing)
                {
                    return;
                }

                statusInfo.IsSyncing = true;

                try
                {
                    Console.WriteLine($"[TableStatusSynchronizationService] Synchronizing table {tableId} status to database: {status}");

                    bool success = await _tableService.UpdateTableStatusAsync(tableId, status);

                    if (success)
                    {
                        Console.WriteLine($"[TableStatusSynchronizationService] Successfully synchronized table {tableId} status to database");
                    }
                    else
                    {
                        Console.WriteLine($"[TableStatusSynchronizationService] Failed to synchronize table {tableId} status to database");
                    }
                }
                finally
                {
                    statusInfo.IsSyncing = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error synchronizing table {tableId} status: {ex.Message}");
            }
            finally
            {
                _syncSemaphore.Release();
            }
        }

        /// <summary>
        /// Initializes table status with immediate synchronization
        /// </summary>
        public async Task InitializeTableStatusAsync(int tableId, int initialItemCount = 0)
        {
            try
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Initializing status for table {tableId} with {initialItemCount} items");

                await UpdateTableItemCountAsync(tableId, initialItemCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error initializing table status: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets current table status from cache
        /// </summary>
        public string GetTableStatus(int tableId)
        {
            if (_tableStatusCache.TryGetValue(tableId, out var statusInfo))
            {
                return statusInfo.Status;
            }
            return "Available";
        }

        /// <summary>
        /// Gets current table item count from cache
        /// </summary>
        public int GetTableItemCount(int tableId)
        {
            if (_tableStatusCache.TryGetValue(tableId, out var statusInfo))
            {
                return statusInfo.ItemCount;
            }
            return 0;
        }

        /// <summary>
        /// Checks if table is occupied
        /// </summary>
        public bool IsTableOccupied(int tableId)
        {
            return GetTableStatus(tableId) == "Occupied";
        }

        /// <summary>
        /// Removes table from tracking
        /// </summary>
        public async Task RemoveTableAsync(int tableId)
        {
            try
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Removing table {tableId} from tracking");

                if (_tableStatusCache.TryRemove(tableId, out var removedTable))
                {
                    if (removedTable.Status == "Occupied")
                    {
                        await _tableService.SetTableAvailableAsync(tableId);
                        Console.WriteLine($"[TableStatusSynchronizationService] Set removed table {tableId} to Available");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error removing table: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all tables with current synchronized status
        /// </summary>
        public async Task<List<RestaurantTable>> GetAllTablesWithCurrentStatusAsync()
        {
            try
            {
                Console.WriteLine("[TableStatusSynchronizationService] Retrieving tables with current status...");

                var tables = await _tableService.GetAllTablesAsync();

                foreach (var table in tables)
                {
                    if (_tableStatusCache.TryGetValue(table.Id, out var cachedInfo))
                    {
                        if (table.Status != cachedInfo.Status)
                        {
                            Console.WriteLine($"[TableStatusSynchronizationService] Status mismatch for table {table.Id}: " +
                                            $"DB='{table.Status}', Cache='{cachedInfo.Status}' - using cache");
                            table.Status = cachedInfo.Status;
                        }
                    }
                }

                Console.WriteLine($"[TableStatusSynchronizationService] Retrieved {tables.Count} tables with synchronized status");
                return tables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error retrieving tables: {ex.Message}");
                return new List<RestaurantTable>();
            }
        }

        /// <summary>
        /// Gets all table item counts
        /// </summary>
        public Dictionary<int, int> GetAllTableItemCounts()
        {
            return _tableStatusCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ItemCount);
        }

        /// <summary>
        /// Gets all table statuses
        /// </summary>
        public Dictionary<int, string> GetAllTableStatuses()
        {
            return _tableStatusCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Status);
        }

        /// <summary>
        /// Forces immediate synchronization of all tables
        /// </summary>
        public async Task ForceFullSynchronizationAsync()
        {
            try
            {
                Console.WriteLine("[TableStatusSynchronizationService] Forcing immediate full synchronization...");

                var statusUpdates = new Dictionary<int, string>();

                foreach (var statusInfo in _tableStatusCache.Values)
                {
                    if (!statusInfo.IsSyncing)
                    {
                        statusUpdates[statusInfo.TableId] = statusInfo.Status;
                    }
                }

                if (statusUpdates.Count > 0)
                {
                    bool success = await _tableService.BulkUpdateTableStatusAsync(statusUpdates);
                    Console.WriteLine($"[TableStatusSynchronizationService] Immediate force sync {(success ? "completed" : "failed")} for {statusUpdates.Count} tables");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error in immediate force synchronization: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears all table statuses and sets them to Available
        /// </summary>
        public async Task ClearAllTableStatusesAsync()
        {
            try
            {
                Console.WriteLine("[TableStatusSynchronizationService] Clearing all table statuses...");

                var tableIds = _tableStatusCache.Keys.ToList();
                var statusUpdates = tableIds.ToDictionary(id => id, id => "Available");

                if (statusUpdates.Count > 0)
                {
                    bool success = await _tableService.BulkUpdateTableStatusAsync(statusUpdates);

                    if (success)
                    {
                        _tableStatusCache.Clear();
                        Console.WriteLine($"[TableStatusSynchronizationService] Cleared {tableIds.Count} table statuses");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error clearing table statuses: {ex.Message}");
            }
        }

        /// <summary>
        /// Raises the table status changed event
        /// </summary>
        protected virtual void OnTableStatusChanged(TableStatusChangedEventArgs e)
        {
            try
            {
                Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    TableStatusChanged?.Invoke(this, e);
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error raising table status changed event: {ex.Message}");
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
                    _disposed = true;
                    _syncSemaphore?.Dispose();
                    _tableService?.Dispose();
                    _tableStatusCache?.Clear();

                    Console.WriteLine("[TableStatusSynchronizationService] Immediate synchronization service disposed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TableStatusSynchronizationService] Error during disposal: {ex.Message}");
                }
            }
        }

        ~TableStatusSynchronizationService()
        {
            Dispose(false);
        }
    }



    /// <summary>
    /// Singleton instance of the table status synchronization service
    /// </summary>
    public static class TableStatusSynchronizationServiceSingleton
    {
        private static readonly Lazy<TableStatusSynchronizationService> _instance =
            new Lazy<TableStatusSynchronizationService>(() => new TableStatusSynchronizationService());

        public static TableStatusSynchronizationService Instance => _instance.Value;

        public static void DisposeInstance()
        {
            if (_instance.IsValueCreated)
            {
                _instance.Value.Dispose();
            }
        }
    }
}