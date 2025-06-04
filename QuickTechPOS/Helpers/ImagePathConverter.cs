using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace QuickTechPOS.Helpers
{
    /// <summary>
    /// Converts image paths to BitmapImage with proper error handling
    /// </summary>
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    return null; // Return null for missing images
                }

                string imagePath = value.ToString();

                // Check if file exists
                if (!File.Exists(imagePath))
                {
                    return null; // Return null if file doesn't exist
                }

                // Create BitmapImage with error handling
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load immediately
                bitmap.EndInit();
                bitmap.Freeze(); // Make it thread-safe

                return bitmap;
            }
            catch
            {
                // Return null on any error
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}