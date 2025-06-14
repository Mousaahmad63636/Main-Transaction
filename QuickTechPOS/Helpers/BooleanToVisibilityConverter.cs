using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QuickTechPOS.Converters
{
    /// <summary>
    /// Converter that converts boolean to Visibility with optional inversion
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Check if we should invert the logic
                bool invert = parameter?.ToString()?.ToLower() == "invert";

                if (invert)
                    boolValue = !boolValue;

                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;

                // Check if we should invert the logic
                bool invert = parameter?.ToString()?.ToLower() == "invert";

                if (invert)
                    result = !result;

                return result;
            }
            return false;
        }
    }
}