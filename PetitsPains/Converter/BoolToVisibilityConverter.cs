using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PetitsPains.Converter
{
    /// <summary>
    /// Converter converting a boolean to a visibility.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean to a visibility object.
        /// </summary>
        /// <param name="value">Boolean value.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">Parameter passed to the converter.</param>
        /// <param name="culture">Culture to use in the converter.</param>
        /// <returns>A visibility object; visible if the boolean is true, collapsed otherwise. The hidden property is not handled.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;   
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Converts a visibility to a boolean.
        /// </summary>
        /// <param name="value">Visibility value.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="language">The culture to use in the converter.</param>
        /// <returns>A boolean; true if the value is Visible, false otherwise. Therefore, hidden and collapsed properties are handled the same way.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return ((Visibility)value) == Visibility.Visible;   
            }
            else
            {
                return false;
            }
        }
    }
}
