using Pixel.Automation.RestApi.Shared;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Pixel.Automation.HttpRequest.Editor;

public class ResponseTypeToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        ExpectedResponse expectedResponse;
        Enum.TryParse<ExpectedResponse>(value?.ToString(), out expectedResponse);
        switch(expectedResponse)
        {
            case ExpectedResponse.File:
                switch(parameter)
                {
                    case "SaveAsOthers":
                        return Visibility.Collapsed;
                    case "SaveAsFile":
                        return Visibility.Visible;
                         
                }
                break;
            default:
                switch (parameter)
                {
                    case "SaveAsOthers":
                        return Visibility.Visible;
                    case "SaveAsFile":
                        return Visibility.Collapsed;
                }
                break;           
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
