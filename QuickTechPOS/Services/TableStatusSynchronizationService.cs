using QuickTechPOS.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace QuickTechPOS.Services
{
    public class TableStatusSynchronizationService : IDisposable
    {
        private readonly RestaurantTableService _tableService;
        private readonly ConcurrentDictionary<int, TableStatusInfo> _tableStatusCache;
        private readonly Timer _syncTimer;
        private readonly object _syncLock = new object();
        private bool _disposed = false;
        private bool _syncInProgress = false;

        public event EventHandler<TableStatusChangedEventArgs> TableStatusChanged;

        private class TableStatusInfo
        {
            public int TableId { get; set; }
            public string Status { get; set; }
            public int ItemCount { get; set; }
            public DateTime LastUpdated { get; set; }
            public bool NeedsSync { get; set; }
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
            Console.WriteLine("[TableStatusSynchronizationService] Initializing enhanced table status synchronization service...");

            _tableService = new RestaurantTableService();
            _tableStatusCache = new ConcurrentDictionary<int, TableStatusInfo>();

            _syncTimer = new Timer(PerformSynchronization, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));

            Console.WriteLine("[TableStatusSynchronizationService] Enhanced table status synchronization service initialized");
        }

        public async Task UpdateTableItemCountAsync(int tableId, int itemCount)
        {
            try
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Updating enhanced item count for table {tableId}: {itemCount} items");

                var statusInfo = _tableStatusCache.GetOrAdd(tableId, id => new TableStatusInfo
                {
                    TableId = id,
                    Status = "Available",
                    ItemCount = 0,
                    LastUpdated = DateTime.Now,
                    NeedsSync = false
                });

                string expectedStatus = itemCount > 0 ? "Occupied" : "Available";

                bool statusChanged = statusInfo.Status != expectedStatus;
                bool itemCountChanged = statusInfo.ItemCount != itemCount;

                if (statusChanged || itemCountChanged)
                {
                    string oldStatus = statusInfo.Status;

                    statusInfo.ItemCount = itemCount;
                    statusInfo.Status = expectedStatus;
                    statusInfo.LastUpdated = DateTime.Now;
                    statusInfo.NeedsSync = true;

                    Console.WriteLine($"[TableStatusSynchronizationService] Enhanced status change detected for table {tableId}: " +
                                     $"Status: '{oldStatus}' -> '{expectedStatus}', Items: {itemCount}");

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
                Console.WriteLine($"[TableStatusSynchronizationService] Error updating enhanced table item count: {ex.Message}");
            }
        }

        public async Task RemoveTableAsync(int tableId)
        {
            try
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Removing enhanced table {tableId} from tracking");

                if (_tableStatusCache.TryRemove(tableId, out var removedTable))
                {
                    if (removedTable.Status == "Occupied")
                    {
                        await _tableService.SetTableAvailableAsync(tableId);
                        Console.WriteLine($"[TableStatusSynchronizationService] Set removed table {tableId} to Available (green)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error removing enhanced table: {ex.Message}");
            }
        }

        public async Task<List<RestaurantTable>> GetAllTablesWithCurrentStatusAsync()
        {
            try
            {
                Console.WriteLine("[TableStatusSynchronizationService] Retrieving enhanced tables with current status...");

                var tables = await _tableService.GetAllTablesAsync();

                foreach (var table in tables)
                {
                    if (_tableStatusCache.TryGetValue(table.Id, out var cachedInfo))
                    {
                        if (table.Status != cachedInfo.Status)
                        {
                            Console.WriteLine($"[TableStatusSynchronizationService] Enhanced status mismatch for table {table.Id}: " +
                                            $"DB='{table.Status}', Cache='{cachedInfo.Status}' - using cache");
                            table.Status = cachedInfo.Status;
                        }
                    }
                }

                Console.WriteLine($"[TableStatusSynchronizationService] Retrieved {tables.Count} enhanced tables with synchronized status");
                return tables;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error retrieving enhanced tables: {ex.Message}");
                return new List<RestaurantTable>();
            }
        }

        public Dictionary<int, int> GetAllTableItemCounts()
        {
            return _tableStatusCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ItemCount);
        }

        public Dictionary<int, string> GetAllTableStatuses()
        {
            return _tableStatusCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Status);
        }

        public async Task ForceFullSynchronizationAsync()
        {
            try
            {
                Console.WriteLine("[TableStatusSynchronizationService] Forcing enhanced full synchronization...");

                lock (_syncLock)
                {
                    if (_syncInProgress)
                    {
                        Console.WriteLine("[TableStatusSynchronizationService] Enhanced sync already in progress, skipping force sync");
                        return;
                    }
                    _syncInProgress = true;
                }

                try
                {
                    var statusUpdates = new Dictionary<int, string>();

                    foreach (var statusInfo in _tableStatusCache.Values)
                    {
                        statusUpdates[statusInfo.TableId] = statusInfo.Status;
                        statusInfo.NeedsSync = false;
                    }

                    if (statusUpdates.Count > 0)
                    {
                        bool success = await _tableService.BulkUpdateTableStatusAsync(statusUpdates);
                        Console.WriteLine($"[TableStatusSynchronizationService] Enhanced force sync {(success ? "completed" : "failed")} for {statusUpdates.Count} tables");
                    }
                }
                finally
                {
                    _syncInProgress = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error in enhanced force synchronization: {ex.Message}");
                _syncInProgress = false;
            }
        }

        private async void PerformSynchronization(object state)
        {
            try
            {
                lock (_syncLock)
                {
                    if (_syncInProgress || _disposed)
                    {
                        return;
                    }
                    _syncInProgress = true;
                }

                try
                {
                    var tablesToSync = _tableStatusCache.Values
                        .Where(info => info.NeedsSync)
                        .ToList();

                    if (tablesToSync.Count > 0)
                    {
                        Console.WriteLine($"[TableStatusSynchronizationService] Enhanced periodic sync: {tablesToSync.Count} tables need updates");

                        var statusUpdates = tablesToSync.ToDictionary(
                            info => info.TableId,
                            info => info.Status
                        );

                        bool success = await _tableService.BulkUpdateTableStatusAsync(statusUpdates);

                        if (success)
                        {
                            foreach (var info in tablesToSync)
                            {
                                info.NeedsSync = false;
                            }
                            Console.WriteLine($"[TableStatusSynchronizationService] Enhanced periodic sync completed successfully for {tablesToSync.Count} tables");
                        }
                        else
                        {
                            Console.WriteLine($"[TableStatusSynchronizationService] Enhanced periodic sync failed for {tablesToSync.Count} tables");
                        }
                    }
                }
                finally
                {
                    _syncInProgress = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error in enhanced periodic synchronization: {ex.Message}");
                _syncInProgress = false;
            }
        }

        public async Task InitializeTableStatusAsync(int tableId, int initialItemCount = 0)
        {
            try
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Initializing enhanced status for table {tableId} with {initialItemCount} items");

                await UpdateTableItemCountAsync(tableId, initialItemCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error initializing enhanced table status: {ex.Message}");
            }
        }

        public bool IsTableOccupied(int tableId)
        {
            if (_tableStatusCache.TryGetValue(tableId, out var statusInfo))
            {
                return statusInfo.Status == "Occupied";
            }
            return false;
        }

        public int GetTableItemCount(int tableId)
        {
            if (_tableStatusCache.TryGetValue(tableId, out var statusInfo))
            {
                return statusInfo.ItemCount;
            }
            return 0;
        }

        public string GetTableStatus(int tableId)
        {
            if (_tableStatusCache.TryGetValue(tableId, out var statusInfo))
            {
                return statusInfo.Status;
            }
            return "Available";
        }

        public async Task ClearAllTableStatusesAsync()
        {
            try
            {
                Console.WriteLine("[TableStatusSynchronizationService] Clearing enhanced all table statuses...");

                var tableIds = _tableStatusCache.Keys.ToList();
                var statusUpdates = tableIds.ToDictionary(id => id, id => "Available");

                if (statusUpdates.Count > 0)
                {
                    bool success = await _tableService.BulkUpdateTableStatusAsync(statusUpdates);

                    if (success)
                    {
                        _tableStatusCache.Clear();
                        Console.WriteLine($"[TableStatusSynchronizationService] Enhanced cleared {tableIds.Count} table statuses");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusSynchronizationService] Error clearing enhanced table statuses: {ex.Message}");
            }
        }

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
                Console.WriteLine($"[TableStatusSynchronizationService] Error raising enhanced table status changed event: {ex.Message}");
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
                    _syncTimer?.Dispose();
                    _tableService?.Dispose();
                    _tableStatusCache?.Clear();

                    Console.WriteLine("[TableStatusSynchronizationService] Enhanced table status synchronization service disposed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TableStatusSynchronizationService] Error during enhanced disposal: {ex.Message}");
                }
            }
        }

        ~TableStatusSynchronizationService()
        {
            Dispose(false);
        }
    }

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