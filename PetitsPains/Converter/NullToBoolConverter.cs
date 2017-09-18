using System;
using System.Globalization;
using System.Windows.Data;

namespace PetitsPains.Converter
{
    /// <summary>
    /// Enum describing the direction to convert a null.
    /// </summary>
    public enum NullToBoolDirection
    {
        NullIsTrue,
        NullIsFalse
    }

    /// <summary>
    /// Converter converting a null to a boolean.
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts a "state" of an object (null or not) to a boolean.
        /// </summary>
        /// <param name="value">An object.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">In which direction do we convert the null?</param>
        /// <param name="culture">Culture to use in the converter.</param>
        /// <returns>A boolean.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((parameter == null) || ((NullToBoolDirection)parameter == NullToBoolDirection.NullIsFalse))
            {
                // Null means "false", or the conversion direction provided is null
                return value == null;
            }
            else
            {
                // Null means "true"
                return value != null;
            }
        }

        /// <summary>
        /// Converts a boolean to an object.<para />
        /// Not yet implemented.
        /// </summary>
        /// <param name="value">Boolean value.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="language">The culture to use in the converter.</param>
        /// <returns>An object.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
