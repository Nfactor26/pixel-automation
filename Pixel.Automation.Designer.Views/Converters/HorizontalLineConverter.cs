using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Pixel.Automation.Designer.Views.Converters
{
    public class HorizontalLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TreeViewItem item = (TreeViewItem)value;
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);            
            int index = ic.ItemContainerGenerator.IndexFromContainer(item);

            //         if ((string)parameter == "left")
            //         {
            //             if (index == 0) // Either left most or single item
            //                 return (int)0;
            //             else
            //                 return (int)1;
            //         }
            //         else // assume "right"
            //         {
            //             if (index == ic.Items.Count - 1)    // Either right most or single item
            //                 return (int)0;
            //             else
            //                 return (int)1;
            //         }

            if ((string)parameter == "left")
            {
                StackPanel parentPanel = VisualTreeHelper.GetParent(item) as StackPanel;
                if(parentPanel!=null)
                {
                    if (parentPanel.Tag != null && parentPanel.Tag.Equals("Contained"))
                        return (int)0;
                }


                if (index == 0)
                {
                    ItemsControl parent = GetSelectedTreeViewItemParent(item);
                    if (parent is TreeView)
                    {
                        return (int)0;
                    }
                }
                //if(index == ic.Items.Count-1)
                //    return (int)0;

                return (int)1;

            }
            else
            {
                if (item.HasItems)
                    return (int)1;
                else
                    return (int)0;
            }




        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public ItemsControl GetSelectedTreeViewItemParent(TreeViewItem item)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as ItemsControl;
        }

    }

}
