// File: QuickTechPOS/Models/RestaurantTable.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickTechPOS.Models
{
    /// <summary>
    /// Represents a restaurant table entity for table management and ordering
    /// </summary>
    public class RestaurantTable
    {
        /// <summary>
        /// Unique identifier for the restaurant table
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Table number for identification and ordering
        /// </summary>
        [Required]
        public int TableNumber { get; set; }

        /// <summary>
        /// Current status of the table (Available, Occupied, Reserved, Out of Service)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Available";

        /// <summary>
        /// Optional description or notes about the table
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the table is active and available for use
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date and time when the table record was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Date and time when the table record was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets the display name for the table
        /// </summary>
        [NotMapped]
        public string DisplayName => $"Table {TableNumber}";

        /// <summary>
        /// Gets a formatted status display string
        /// </summary>
        [NotMapped]
        public string StatusDisplay => Status?.ToString() ?? "Unknown";

        /// <summary>
        /// Gets whether the table is available for new orders
        /// </summary>
        [NotMapped]
        public bool IsAvailable => IsActive &&
                                  string.Equals(Status, "Available", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets whether the table is currently occupied
        /// </summary>
        [NotMapped]
        public bool IsOccupied => IsActive &&
                                 string.Equals(Status, "Occupied", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets whether the table is reserved
        /// </summary>
        [NotMapped]
        public bool IsReserved => IsActive &&
                                 string.Equals(Status, "Reserved", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the CSS class for status display based on table status
        /// </summary>
        [NotMapped]
        public string StatusClass => Status?.ToLower() switch
        {
            "available" => "status-available",
            "occupied" => "status-occupied",
            "reserved" => "status-reserved",
            "out of service" => "status-outofservice",
            _ => "status-unknown"
        };

        /// <summary>
        /// Gets a detailed information string about the table
        /// </summary>
        [NotMapped]
        public string TableInfo
        {
            get
            {
                var info = $"Table {TableNumber} - {Status}";
                if (!string.IsNullOrWhiteSpace(Description))
                {
                    info += $" ({Description})";
                }
                return info;
            }
        }

        /// <summary>
        /// Updates the table status and sets the UpdatedAt timestamp
        /// </summary>
        /// <param name="newStatus">The new status to set</param>
        public void UpdateStatus(string newStatus)
        {
            if (!string.IsNullOrWhiteSpace(newStatus) && Status != newStatus)
            {
                Status = newStatus;
                UpdatedAt = DateTime.Now;
            }
        }

        /// <summary>
        /// Marks the table as available and updates the timestamp
        /// </summary>
        public void SetAvailable()
        {
            UpdateStatus("Available");
        }

        /// <summary>
        /// Marks the table as occupied and updates the timestamp
        /// </summary>
        public void SetOccupied()
        {
            UpdateStatus("Occupied");
        }

        /// <summary>
        /// Marks the table as reserved and updates the timestamp
        /// </summary>
        public void SetReserved()
        {
            UpdateStatus("Reserved");
        }

        /// <summary>
        /// Marks the table as out of service and updates the timestamp
        /// </summary>
        public void SetOutOfService()
        {
            UpdateStatus("Out of Service");
        }

        /// <summary>
        /// Validates the table data
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValid()
        {
            return TableNumber > 0 &&
                   !string.IsNullOrWhiteSpace(Status) &&
                   IsValidStatus(Status);
        }

        /// <summary>
        /// Checks if the provided status is a valid table status
        /// </summary>
        /// <param name="status">Status to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            var validStatuses = new[] { "Available", "Occupied", "Reserved", "Out of Service" };
            return Array.Exists(validStatuses, s =>
                string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all valid table statuses
        /// </summary>
        /// <returns>Array of valid status strings</returns>
        public static string[] GetValidStatuses()
        {
            return new[] { "Available", "Occupied", "Reserved", "Out of Service" };
        }

        /// <summary>
        /// Creates a copy of the current table
        /// </summary>
        /// <returns>A new RestaurantTable instance with the same properties</returns>
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

        /// <summary>
        /// Returns a string representation of the table
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return TableInfo;
        }

        /// <summary>
        /// Determines equality based on Id
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is RestaurantTable other)
            {
                return Id == other.Id;
            }
            return false;
        }

        /// <summary>
        /// Gets hash code based on Id
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}