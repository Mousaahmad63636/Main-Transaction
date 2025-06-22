using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QuickTechPOS.Converters
{
    /// <summary>
    /// Converts table status to background color for visual feedback
    /// </summary>
    public class TableStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToLowerInvariant())
                {
                    case "occupied":
                        // Red background for occupied tables
                        return new SolidColorBrush(Color.FromRgb(220, 53, 69));
                    case "available":
                        // Green background for available tables
                        return new SolidColorBrush(Color.FromRgb(40, 167, 69));
                    case "reserved":
                        // Orange background for reserved tables
                        return new SolidColorBrush(Color.FromRgb(255, 193, 7));
                    case "cleaning":
                        // Gray background for cleaning tables
                        return new SolidColorBrush(Color.FromRgb(108, 117, 125));
                    default:
                        // Default gray for unknown status
                        return new SolidColorBrush(Color.FromRgb(173, 181, 189));
                }
            }

            // Default fallback color
            return new SolidColorBrush(Color.FromRgb(173, 181, 189));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not supported for TableStatusToColorConverter");
        }
    }

    /// <summary>
    /// Converts table status to text color for better readability
    /// </summary>
    public class TableStatusToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToLowerInvariant())
                {
                    case "occupied":
                        // White text on red background
                        return new SolidColorBrush(Colors.White);
                    case "available":
                        // White text on green background
                        return new SolidColorBrush(Colors.White);
                    case "reserved":
                        // Dark text on orange background
                        return new SolidColorBrush(Color.FromRgb(52, 58, 64));
                    case "cleaning":
                        // White text on gray background
                        return new SolidColorBrush(Colors.White);
                    default:
                        // Dark text for unknown status
                        return new SolidColorBrush(Color.FromRgb(52, 58, 64));
                }
            }

            // Default text color
            return new SolidColorBrush(Color.FromRgb(52, 58, 64));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not supported for TableStatusToTextColorConverter");
        }
    }

    /// <summary>
    /// Converts table status to border color for additional visual feedback
    /// </summary>
    public class TableStatusToBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToLowerInvariant())
                {
                    case "occupied":
                        // Darker red border for occupied tables
                        return new SolidColorBrush(Color.FromRgb(183, 28, 28));
                    case "available":
                        // Darker green border for available tables
                        return new SolidColorBrush(Color.FromRgb(27, 94, 32));
                    case "reserved":
                        // Darker orange border for reserved tables
                        return new SolidColorBrush(Color.FromRgb(230, 81, 0));
                    case "cleaning":
                        // Darker gray border for cleaning tables
                        return new SolidColorBrush(Color.FromRgb(69, 90, 100));
                    default:
                        // Default border color
                        return new SolidColorBrush(Color.FromRgb(121, 85, 72));
                }
            }

            // Default border color
            return new SolidColorBrush(Color.FromRgb(121, 85, 72));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not supported for TableStatusToBorderColorConverter");
        }
    }
}