using Pixel.Automation.Core.Arguments;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Pixel.Automation.Arguments.Editor
{
    public class ArgumentModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ArgumentMode argumentMode;
            Enum.TryParse<ArgumentMode>(value.ToString(), out argumentMode);
            switch (argumentMode)
            {
                case ArgumentMode.Default:
                    return null;
                case ArgumentMode.DataBound:
                    return false;
                case ArgumentMode.Scripted:
                    return true;
                default:
                    return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return ArgumentMode.Default;
            switch(bool.Parse(value.ToString()))
            {
                case true:
                    return ArgumentMode.Scripted;
                case false:
                    return ArgumentMode.DataBound;
                default:
                    return ArgumentMode.Default;

            }
        }
    }
}
