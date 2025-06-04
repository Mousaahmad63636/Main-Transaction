// File: QuickTechPOS/Helpers/StringToVisibilityConverter.cs

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QuickTechPOS
{
    /// <summary>
    /// Converts string values to Visibility based on string content and optional parameters
    /// Supports showing/hiding UI elements based on whether strings are null, empty, or contain specific values
    /// Enhanced version with parameter support for inverse logic and custom matching
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a string value to Visibility enum
        /// </summary>
        /// <param name="value">The string value to evaluate</param>
        /// <param name="targetType">Target type (should be Visibility)</param>
        /// <param name="parameter">
        /// Optional parameter to control behavior:
        /// - "Inverse" or "Invert": Returns Collapsed for non-empty strings, Visible for empty/null
        /// - "HideOnEmpty": Returns Collapsed for empty/null strings (default behavior)
        /// - Any other string: Returns Visible if the value contains the parameter string
        /// - null: Uses default behavior (visible when not empty)
        /// </param>
        /// <param name="culture">Culture information</param>
        /// <returns>Visibility.Visible or Visibility.Collapsed</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string stringValue = value as string;
                string parameterValue = parameter as string;

                // Handle inverse logic
                if (string.Equals(parameterValue, "Inverse", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(parameterValue, "Invert", StringComparison.OrdinalIgnoreCase))
                {
                    // Return Visible for empty/null, Collapsed for non-empty
                    return string.IsNullOrWhiteSpace(stringValue) ? Visibility.Visible : Visibility.Collapsed;
                }

                // Handle specific string matching
                if (!string.IsNullOrEmpty(parameterValue) &&
                    !string.Equals(parameterValue, "HideOnEmpty", StringComparison.OrdinalIgnoreCase))
                {
                    // Return Visible if the string contains the parameter value
                    return !string.IsNullOrEmpty(stringValue) &&
                           stringValue.IndexOf(parameterValue, StringComparison.OrdinalIgnoreCase) >= 0
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }

                // Default behavior: Return Visible for non-empty strings, Collapsed for empty/null
                return !string.IsNullOrWhiteSpace(stringValue) ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                // Log the error for debugging but don't crash the UI
                System.Diagnostics.Debug.WriteLine($"StringToVisibilityConverter error: {ex.Message}");

                // Return a safe default
                return Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Converts back from Visibility to string (not typically used)
        /// </summary>
        /// <param name="value">Visibility value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">Conversion parameter</param>
        /// <param name="culture">Culture information</param>
        /// <returns>String representation or null</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // ConvertBack is not typically needed for this converter
            // Return null or empty string based on visibility
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible ? "visible" : string.Empty;
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Specialized converter for handling image paths and providing fallback logic
    /// Determines whether to show the actual image or a placeholder based on image path availability
    /// </summary>
    public class ImagePathToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts an image path to visibility, with support for checking file existence
        /// </summary>
        /// <param name="value">The image path string</param>
        /// <param name="targetType">Target type (should be Visibility)</param>
        /// <param name="parameter">
        /// Optional parameter:
        /// - "ShowPlaceholder": Returns Visible when image path is invalid (for placeholder display)
        /// - "ShowImage": Returns Visible when image path is valid (for actual image display) - default
        /// - "CheckExists": Also verifies if the file actually exists on disk
        /// </param>
        /// <param name="culture">Culture information</param>
        /// <returns>Visibility.Visible or Visibility.Collapsed</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string imagePath = value as string;
                string parameterValue = parameter as string;

                bool hasValidPath = !string.IsNullOrWhiteSpace(imagePath);
                bool showPlaceholder = string.Equals(parameterValue, "ShowPlaceholder", StringComparison.OrdinalIgnoreCase);
                bool checkExists = string.Equals(parameterValue, "CheckExists", StringComparison.OrdinalIgnoreCase);

                // If checking file existence is requested
                if (checkExists && hasValidPath)
                {
                    try
                    {
                        // Check if it's a pack URI or file path
                        if (imagePath.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                        {
                            hasValidPath = true; // Assume pack URIs are valid
                        }
                        else
                        {
                            hasValidPath = System.IO.File.Exists(imagePath);
                        }
                    }
                    catch
                    {
                        hasValidPath = false;
                    }
                }

                // Return appropriate visibility based on parameter
                if (showPlaceholder)
                {
                    return hasValidPath ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return hasValidPath ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ImagePathToVisibilityConverter error: {ex.Message}");

                // Safe fallback
                string parameterValue = parameter as string;
                bool showPlaceholder = string.Equals(parameterValue, "ShowPlaceholder", StringComparison.OrdinalIgnoreCase);
                return showPlaceholder ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Converts back from Visibility to image path (not typically used)
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not implemented as it's rarely needed for image path scenarios
            throw new NotImplementedException("ConvertBack not supported for ImagePathToVisibilityConverter");
        }
    }

    /// <summary>
    /// Converter that handles numeric values and converts them to Visibility
    /// Useful for showing elements based on stock quantities, counts, etc.
    /// </summary>
    public class NumericToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts numeric values to Visibility
        /// </summary>
        /// <param name="value">The numeric value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="parameter">
        /// Optional parameter:
        /// - "HideZero": Hide when value is 0 (default)
        /// - "ShowZero": Show when value is 0
        /// - "GreaterThan:X": Show when value is greater than X
        /// - "LessThan:X": Show when value is less than X
        /// </param>
        /// <param name="culture">Culture information</param>
        /// <returns>Visibility.Visible or Visibility.Collapsed</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return Visibility.Collapsed;

                double numericValue = System.Convert.ToDouble(value);
                string parameterValue = parameter as string;

                if (string.IsNullOrEmpty(parameterValue) || parameterValue == "HideZero")
                {
                    return numericValue > 0 ? Visibility.Visible : Visibility.Collapsed;
                }

                if (parameterValue == "ShowZero")
                {
                    return numericValue == 0 ? Visibility.Visible : Visibility.Collapsed;
                }

                if (parameterValue.StartsWith("GreaterThan:", StringComparison.OrdinalIgnoreCase))
                {
                    if (double.TryParse(parameterValue.Substring(12), out double threshold))
                    {
                        return numericValue > threshold ? Visibility.Visible : Visibility.Collapsed;
                    }
                }

                if (parameterValue.StartsWith("LessThan:", StringComparison.OrdinalIgnoreCase))
                {
                    if (double.TryParse(parameterValue.Substring(9), out double threshold))
                    {
                        return numericValue < threshold ? Visibility.Visible : Visibility.Collapsed;
                    }
                }

                // Default: show if greater than 0
                return numericValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NumericToVisibilityConverter error: {ex.Message}");
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}