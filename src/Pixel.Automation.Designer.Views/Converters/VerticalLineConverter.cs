using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Pixel.Automation.Designer.Views.Converters
{

    public class VerticalLineConverter : IMultiValueConverter
    {      
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int siblingCount = System.Convert.ToInt32(values[0]);
            int index = System.Convert.ToInt32(values[1]);

            TreeViewItem item = (TreeViewItem)(values[2]);          
            StackPanel parentPanel = VisualTreeHelper.GetParent(item) as StackPanel;
            if (parentPanel?.Tag?.Equals("Contained") ?? false)
            {              
                return (double)0;
            }

            if (index < siblingCount)
            {
                return (double)1;
            }
            return (double)0;
        }
       
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { Binding.DoNothing };
        }
    }

}
