using System;
using System.Globalization;
using System.Windows.Data;

namespace PetitsPains.Converter
{
    /// <summary>
    /// Converter converting a string to a boolean.
    /// </summary>
    class StringNullOrEmptyToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts a "state" of a string (empty/null or not) to a boolean.
        /// </summary>
        /// <param name="value">A string.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">Culture to use in the converter.</param>
        /// <returns>A boolean object: true is the string is not null or empty, false otherwise. The hidden property is not handled.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Null or empty means "false"
            return !String.IsNullOrWhiteSpace(value as string);
        }

        /// <summary>
        /// Converts a boolean to a string.<para />
        /// Not yet implemented.
        /// </summary>
        /// <param name="value">Boolean value.</param>
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
