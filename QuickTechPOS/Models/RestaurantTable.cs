using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

namespace QuickTechPOS.Models
{
    [Table("RestaurantTables")]
    public class RestaurantTable : INotifyPropertyChanged
    {
        private string _status;
        private string _description;
        private bool _isActive;
        private DateTime? _updatedAt;

        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Table number must be greater than 0")]
        public int TableNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    _updatedAt = DateTime.Now;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsAvailable));
                    OnPropertyChanged(nameof(IsOccupied));
                    OnPropertyChanged(nameof(IsReserved));
                    OnPropertyChanged(nameof(StatusColor));
                    OnPropertyChanged(nameof(StatusIcon));
                    OnPropertyChanged(nameof(DisplayName));
                    OnPropertyChanged(nameof(TableInfo));
                }
            }
        }

        [StringLength(255)]
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    _updatedAt = DateTime.Now;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayName));
                    OnPropertyChanged(nameof(TableInfo));
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    _updatedAt = DateTime.Now;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt
        {
            get => _updatedAt;
            set
            {
                if (_updatedAt != value)
                {
                    _updatedAt = value;
                    OnPropertyChanged();
                }
            }
        }

        [NotMapped]
        public string DisplayName => $"Table {TableNumber}";

        [NotMapped]
        public string TableInfo
        {
            get
            {
                var info = DisplayName;
                if (!string.IsNullOrEmpty(Description))
                {
                    info += $" - {Description}";
                }
                info += $" ({Status})";
                return info;
            }
        }

        [NotMapped]
        public bool IsAvailable => string.Equals(Status, "Available", StringComparison.OrdinalIgnoreCase);

        [NotMapped]
        public bool IsOccupied => string.Equals(Status, "Occupied", StringComparison.OrdinalIgnoreCase);

        [NotMapped]
        public bool IsReserved => string.Equals(Status, "Reserved", StringComparison.OrdinalIgnoreCase);

        [NotMapped]
        public bool IsOutOfService => string.Equals(Status, "Out of Service", StringComparison.OrdinalIgnoreCase);

        [NotMapped]
        public string StatusColor
        {
            get
            {
                return Status?.ToLower() switch
                {
                    "available" => "#10B981",
                    "occupied" => "#EF4444",
                    "reserved" => "#F59E0B",
                    "out of service" => "#6B7280",
                    _ => "#9CA3AF"
                };
            }
        }

        [NotMapped]
        public string StatusIcon
        {
            get
            {
                return Status?.ToLower() switch
                {
                    "available" => "✓",
                    "occupied" => "🔴",
                    "reserved" => "📋",
                    "out of service" => "⚠",
                    _ => "❓"
                };
            }
        }

        [NotMapped]
        public string StatusDisplayText
        {
            get
            {
                return Status?.ToLower() switch
                {
                    "available" => "Available (Green)",
                    "occupied" => "Occupied (Red - Has Items)",
                    "reserved" => "Reserved (Orange)",
                    "out of service" => "Out of Service (Gray)",
                    _ => Status ?? "Unknown"
                };
            }
        }

        [NotMapped]
        public static readonly HashSet<string> ValidStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Available",
            "Occupied",
            "Reserved",
            "Out of Service"
        };

        public static bool IsValidStatus(string status)
        {
            return !string.IsNullOrWhiteSpace(status) && ValidStatuses.Contains(status);
        }

        public void UpdateStatus(string newStatus)
        {
            if (!IsValidStatus(newStatus))
            {
                throw new ArgumentException($"Invalid table status: {newStatus}. Valid statuses are: {string.Join(", ", ValidStatuses)}");
            }

            var oldStatus = Status;
            Status = newStatus;
            UpdatedAt = DateTime.Now;

            Console.WriteLine($"[RestaurantTable] Enhanced status update for Table {TableNumber}: '{oldStatus}' -> '{newStatus}' at {UpdatedAt}");
        }

        public void SetOccupied()
        {
            UpdateStatus("Occupied");
            Console.WriteLine($"[RestaurantTable] Table {TableNumber} set to Occupied (Red - Has Items)");
        }

        public void SetAvailable()
        {
            UpdateStatus("Available");
            Console.WriteLine($"[RestaurantTable] Table {TableNumber} set to Available (Green - No Items)");
        }

        public void SetReserved()
        {
            UpdateStatus("Reserved");
            Console.WriteLine($"[RestaurantTable] Table {TableNumber} set to Reserved (Orange)");
        }

        public void SetOutOfService()
        {
            UpdateStatus("Out of Service");
            Console.WriteLine($"[RestaurantTable] Table {TableNumber} set to Out of Service (Gray)");
        }

        public bool ShouldBeOccupied(int itemCount)
        {
            return itemCount > 0;
        }

        public bool ShouldBeAvailable(int itemCount)
        {
            return itemCount == 0 && (IsOccupied || IsAvailable);
        }

        public string DetermineStatusByItems(int itemCount)
        {
            if (itemCount > 0)
            {
                return "Occupied";
            }
            else if (itemCount == 0 && IsOccupied)
            {
                return "Available";
            }
            else
            {
                return Status;
            }
        }

        public bool NeedsStatusUpdate(int itemCount)
        {
            var expectedStatus = DetermineStatusByItems(itemCount);
            return !string.Equals(Status, expectedStatus, StringComparison.OrdinalIgnoreCase);
        }

        public void UpdateStatusBasedOnItems(int itemCount)
        {
            var newStatus = DetermineStatusByItems(itemCount);
            if (NeedsStatusUpdate(itemCount))
            {
                var oldStatus = Status;
                UpdateStatus(newStatus);
                Console.WriteLine($"[RestaurantTable] Auto-updated Table {TableNumber} status from '{oldStatus}' to '{newStatus}' based on {itemCount} items");
            }
        }

        public override string ToString()
        {
            return $"Table {TableNumber} ({Status})";
        }

        public override bool Equals(object obj)
        {
            if (obj is RestaurantTable other)
            {
                return Id == other.Id && TableNumber == other.TableNumber;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, TableNumber);
        }

        public RestaurantTable Clone()
        {
            return new RestaurantTable
            {
                Id = this.Id,
                TableNumber = this.TableNumber,
                Status = this.Status,
                Description = this.Description,
                IsActive = this.IsActive,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt
            };
        }

        public Dictionary<string, object> ToStatusUpdate()
        {
            return new Dictionary<string, object>
            {
                ["TableId"] = Id,
                ["TableNumber"] = TableNumber,
                ["Status"] = Status,
                ["UpdatedAt"] = UpdatedAt ?? DateTime.Now,
                ["IsOccupied"] = IsOccupied,
                ["IsAvailable"] = IsAvailable,
                ["StatusColor"] = StatusColor,
                ["DisplayName"] = DisplayName
            };
        }

        public void Validate()
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(this);

            if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
            {
                var errors = validationResults.Select(vr => vr.ErrorMessage);
                throw new ValidationException($"Table validation failed: {string.Join(", ", errors)}");
            }

            if (!IsValidStatus(Status))
            {
                throw new ValidationException($"Invalid status '{Status}'. Valid statuses: {string.Join(", ", ValidStatuses)}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class StatusChangeEventArgs : EventArgs
        {
            public string OldStatus { get; set; }
            public string NewStatus { get; set; }
            public DateTime ChangeTime { get; set; }
            public string Reason { get; set; }

            public StatusChangeEventArgs(string oldStatus, string newStatus, string reason = "")
            {
                OldStatus = oldStatus;
                NewStatus = newStatus;
                ChangeTime = DateTime.Now;
                Reason = reason;
            }
        }

        public event EventHandler<StatusChangeEventArgs> StatusChanged;

        protected virtual void OnStatusChanged(string oldStatus, string newStatus, string reason = "")
        {
            StatusChanged?.Invoke(this, new StatusChangeEventArgs(oldStatus, newStatus, reason));
        }
    }

    public static class RestaurantTableExtensions
    {
        public static IEnumerable<RestaurantTable> GetOccupiedTables(this IEnumerable<RestaurantTable> tables)
        {
            return tables.Where(t => t.IsOccupied);
        }

        public static IEnumerable<RestaurantTable> GetAvailableTables(this IEnumerable<RestaurantTable> tables)
        {
            return tables.Where(t => t.IsAvailable);
        }

        public static IEnumerable<RestaurantTable> GetReservedTables(this IEnumerable<RestaurantTable> tables)
        {
            return tables.Where(t => t.IsReserved);
        }

        public static IEnumerable<RestaurantTable> GetActiveTables(this IEnumerable<RestaurantTable> tables)
        {
            return tables.Where(t => t.IsActive);
        }

        public static Dictionary<string, int> GetStatusCounts(this IEnumerable<RestaurantTable> tables)
        {
            var activeTables = tables.Where(t => t.IsActive);
            return new Dictionary<string, int>
            {
                ["Total"] = activeTables.Count(),
                ["Available"] = activeTables.Count(t => t.IsAvailable),
                ["Occupied"] = activeTables.Count(t => t.IsOccupied),
                ["Reserved"] = activeTables.Count(t => t.IsReserved),
                ["OutOfService"] = activeTables.Count(t => t.IsOutOfService)
            };
        }

        public static void BulkUpdateStatusBasedOnItems(this IEnumerable<RestaurantTable> tables, Dictionary<int, int> tableItemCounts)
        {
            foreach (var table in tables)
            {
                if (tableItemCounts.TryGetValue(table.Id, out int itemCount))
                {
                    table.UpdateStatusBasedOnItems(itemCount);
                }
            }
        }

        public static IEnumerable<RestaurantTable> FilterByStatus(this IEnumerable<RestaurantTable> tables, string status)
        {
            if (string.IsNullOrWhiteSpace(status) || status.Equals("All Statuses", StringComparison.OrdinalIgnoreCase))
            {
                return tables;
            }

            return tables.Where(t => string.Equals(t.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<RestaurantTable> SearchTables(this IEnumerable<RestaurantTable> tables, string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return tables;
            }

            var query = searchQuery.ToLower();
            return tables.Where(t =>
                t.TableNumber.ToString().Contains(query) ||
                (t.Description?.ToLower().Contains(query) == true));
        }
    }
}