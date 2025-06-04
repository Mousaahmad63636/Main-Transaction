using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QuickTechPOS
{
    /// <summary>
    /// Converts a boolean value to a Visibility value (true becomes Visible, false becomes Collapsed)
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean to a Visibility (true to Visible, false to Collapsed)
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility to a boolean (Visible to true, Collapsed to false)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }

            return false;
        }
    }
}