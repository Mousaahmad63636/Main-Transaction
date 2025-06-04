// File: QuickTechPOS/Converters/DiscrepancySeverityConverters.cs

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QuickTechPOS
{
    /// <summary>
    /// Converts discrepancy severity to background color
    /// </summary>
    public class DiscrepancySeverityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string severity)
            {
                return severity switch
                {
                    "None" => new SolidColorBrush(Color.FromRgb(34, 197, 94)), // Green
                    "Minor" => new SolidColorBrush(Color.FromRgb(251, 191, 36)), // Yellow
                    "Moderate" => new SolidColorBrush(Color.FromRgb(249, 115, 22)), // Orange
                    "Major" => new SolidColorBrush(Color.FromRgb(239, 68, 68)), // Red
                    _ => new SolidColorBrush(Color.FromRgb(156, 163, 175)) // Gray
                };
            }
            return new SolidColorBrush(Color.FromRgb(156, 163, 175));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts discrepancy severity to text color
    /// </summary>
    public class DiscrepancySeverityToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string severity)
            {
                return severity switch
                {
                    "None" => new SolidColorBrush(Color.FromRgb(34, 197, 94)), // Green
                    "Minor" => new SolidColorBrush(Color.FromRgb(217, 119, 6)), // Amber
                    "Moderate" => new SolidColorBrush(Color.FromRgb(234, 88, 12)), // Orange
                    "Major" => new SolidColorBrush(Color.FromRgb(220, 38, 38)), // Red
                    _ => new SolidColorBrush(Color.FromRgb(71, 85, 105)) // Slate
                };
            }
            return new SolidColorBrush(Color.FromRgb(71, 85, 105));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}