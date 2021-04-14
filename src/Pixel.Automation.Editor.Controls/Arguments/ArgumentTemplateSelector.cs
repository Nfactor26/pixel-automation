using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Controls.Arguments
{
    public class ArgumentTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate  SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (item != null && element != null)
            {               
                string typeName = item.GetType().Name;
                DataTemplate dataTemplate = element.TryFindResource($"Argument_{typeName}") as DataTemplate;
                return dataTemplate ?? element.FindResource("Argument_Default") as DataTemplate;
             
            }
            return element.FindResource("Argument_Error") as DataTemplate;
        }

    }
}
