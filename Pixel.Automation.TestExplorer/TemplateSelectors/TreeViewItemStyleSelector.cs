using Pixel.Automation.TestExplorer.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.TestExplorer
{
    public class TreeViewItemStyleSelector : StyleSelector
    {
        public Style TestFixtureStyle { get; set; }

        public Style TestCaseStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item is TestCaseViewModel)
                {
                    return TestCaseStyle;
                }
                if (item is TestFixtureViewModel)
                {
                    return TestFixtureStyle;
                }              
            }
            return null;
        }
    }
}
