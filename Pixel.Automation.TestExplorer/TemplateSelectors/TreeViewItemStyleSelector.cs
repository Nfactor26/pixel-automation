using Pixel.Automation.Core.TestData;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.TestExplorer
{
    public class TreeViewItemStyleSelector : StyleSelector
    {
        public Style TestCategoryStyle { get; set; }

        public Style TestCaseStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null)
            {
                if (item is TestCase)
                {
                    return TestCaseStyle;
                }
                if (item is TestCategory)
                {
                    return TestCategoryStyle;
                }              
            }
            return null;
        }
    }
}
