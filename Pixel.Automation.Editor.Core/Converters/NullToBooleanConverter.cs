using System;
using System.Globalization;
using System.Windows.Data;

namespace Pixel.Automation.Editor.Core.Converters
{
    /// <summary>
    /// Return false if value is null else true.
    /// </summary>
    public class NullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           return (value == null) ? false :  true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
