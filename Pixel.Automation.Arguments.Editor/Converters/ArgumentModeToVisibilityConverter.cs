using Pixel.Automation.Core.Arguments;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Pixel.Automation.Arguments.Editor
{
    public class ArgumentModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ArgumentMode argumentMode;
            Enum.TryParse<ArgumentMode>(value?.ToString(), out argumentMode);
			switch(argumentMode)
            {
                case ArgumentMode.DataBound:
					switch(parameter)
                    {
                        case "VisibleOnDataBound":
                            return Visibility.Visible;
                        case "VisibleOnScripted":
                            return Visibility.Collapsed;
                        default:
                            return Visibility.Collapsed;
                    }                   
                case ArgumentMode.Scripted:
                    switch (parameter)
                    {
                        case "VisibleOnDataBound":
                            return Visibility.Collapsed;
                        case "VisibleOnScripted":
                            return Visibility.Visible;
                        default:
                            return Visibility.Collapsed;
                    }
                case ArgumentMode.Default:
                    switch (parameter)
                    {
                        case "VisibleOnDataBound":                            
                        case "VisibleOnScripted":
                            return Visibility.Collapsed;
                        default:
                            return Visibility.Visible;
                    }
                default:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
