using Pixel.Automation.Core;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Designer.Views
{
    public class ComponentStyleSelector : StyleSelector
    {

        public override Style SelectStyle(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;        

            if (element != null && item != null)
            {

                string typeName = item.GetType().Name;
                string styleKey = TemplateMapper.GetStyleKey(typeName);
                if(!string.IsNullOrEmpty(styleKey))
                {
                    Style componentStyle = element.TryFindResource(styleKey) as Style;
                    return componentStyle;
                }
                else
                {
                   if(item is ActorComponent || item is ServiceComponent)
                    {
                        Style actorDefaultStyle = element.FindResource("SlimStyle") as Style;
                        return actorDefaultStyle;
                    }
                   Style style = element.FindResource("DefaultStyle") as Style;
                   return style;
                }             
               
            }

            return null;
        }
    }
}
