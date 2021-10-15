using Pixel.Automation.Core;
using Pixel.Automation.Editor.Core.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Designer.Views
{
    /// <summary>
    /// StyleSelector resonsible for selecting the correct style for a given component.
    /// Mapping is stored in TemplateMapping.xml. If no mapping exists, default style is provided.
    /// </summary>
    public class ComponentStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ComponentViewModel cvm && container is FrameworkElement element)
            {
                string typeName = cvm.Model.GetType().Name;
                string styleKey = TemplateMapper.GetStyleKey(typeName);
                if (!string.IsNullOrEmpty(styleKey))
                {
                    Style componentStyle = element.TryFindResource(styleKey) as Style;
                    return componentStyle;
                }
                else if(cvm.Model is ActorComponent || cvm.Model is ServiceComponent)
                {
                    Style actorDefaultStyle = element.FindResource("SlimStyle") as Style;
                    return actorDefaultStyle;
                }
                else
                {                    
                    Style style = element.FindResource("DefaultStyle") as Style;
                    return style;
                }
            }

            return null;
        }
    }
}
