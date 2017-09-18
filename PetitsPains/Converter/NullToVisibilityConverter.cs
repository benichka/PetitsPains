using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PetitsPains.Converter
{
    /// <summary>
    /// Enum describing the direction to convert a null.
    /// </summary>
    public enum NullToVisibilityDirection
    {
        NullIsVisible,
        NullIsCollapsed
    }

    /// <summary>
    /// Converter converting a null to a visibility.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a "state" of an object (null or not) to a visibility object.
        /// </summary>
        /// <param name="value">An object.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">In which direction do we convert the null?</param>
        /// <param name="culture">Culture to use in the converter.</param>
        /// <returns>A visibility object; The hidden property is not handled.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((parameter == null) || ((NullToVisibilityDirection)parameter == NullToVisibilityDirection.NullIsCollapsed))
            {
                // Null means "hide this", or the conversion direction is not provided
                return value != null ? Visibility.Visible : Visibility.Collapsed;   
            }
            else
            {
                // Null means "show this"
                return value != null ? Visibility.Collapsed : Visibility.Visible;  
            }
        }

        /// <summary>
        /// Converts a visibility to an object.<para />
        /// Not yet implemented.
        /// </summary>
        /// <param name="value">Visibility value.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="language">The culture to use in the converter.</param>
        /// <returns>An object. Hidden and collapsed properties are handled the same way.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
