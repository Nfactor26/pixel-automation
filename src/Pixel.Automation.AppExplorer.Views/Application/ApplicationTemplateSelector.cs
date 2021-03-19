using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.AppExplorer.Views.Application
{
    public class ApplicationTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (element != null && item != null)
            {
                string typeName = item.GetType().Name;
                DataTemplate componentTemplate = element.TryFindResource(typeName) as DataTemplate;
                return componentTemplate ?? element.FindResource("DefaultApplication") as DataTemplate;
            }
            return null;
        }
    }
}
