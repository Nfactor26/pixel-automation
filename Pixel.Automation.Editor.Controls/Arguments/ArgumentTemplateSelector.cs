using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Controls.Arguments
{
    public class InArgumentTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate  SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (item!=null && element != null)
            {               
                string typeName = item.GetType().Name;
                DataTemplate dataTemplate = element.TryFindResource($"InArgument_{typeName}") as DataTemplate;
                return dataTemplate ?? element.FindResource("InArgument_Default") as DataTemplate;
             
            }
            return base.SelectTemplate(item,container);
        }

    }
}
