using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Pixel.Automation.Editor.Core.Converters
{
    /// <summary>
    /// Return true if value is null else false.
    /// </summary>
    public class NullToInverseBooleanconverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null) ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
