using System;
using System.Globalization;
using System.Windows.Data;

namespace Pixel.Automation.Editor.Core.Converters
{
    public class TrimmedTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null && parameter != null && int.TryParse(parameter.ToString(), out int maxAllowedLength))
            {
                string text = value.ToString();
                if(text.Length >= maxAllowedLength-3)
                {
                    return (text.Substring(0, maxAllowedLength - 3).PadRight(maxAllowedLength, '.'));
                }
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
