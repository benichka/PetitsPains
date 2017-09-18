using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PetitsPains.Converter
{
    /// <summary>
    /// Converter converting a string to a visibility.
    /// </summary>
    class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a "state" of a string (empty/null or not) to a visibility object.
        /// </summary>
        /// <param name="value">A string.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">Culture to use in the converter.</param>
        /// <returns>A visibility object; The hidden property is not handled.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Null or empty means "hide this"
            return String.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Converts a visibility to a string.<para />
        /// Not yet implemented.
        /// </summary>
        /// <param name="value">Visibility value.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="language">The culture to use in the converter.</param>
        /// <returns>A string.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
