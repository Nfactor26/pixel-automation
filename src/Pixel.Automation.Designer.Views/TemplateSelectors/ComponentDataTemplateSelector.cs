using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Processors;
using Pixel.Automation.Editor.Core.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Designer.Views
{
    /// <summary>
    /// DataTempalteSelector resonsible for selecting the correct data template for a given component.
    /// Mapping is stored in TemplateMapping.xml. If no mapping exists, default template is provided.
    /// </summary>
    public class ComponentDataTemplateSelector : DataTemplateSelector
    {      
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ComponentViewModel cvm && container is FrameworkElement element)
            {
                string typeName = cvm.Model.GetType().Name;
                string dataTemplateKey = TemplateMapper.GetDataTemplateKey(typeName);
                if (!string.IsNullOrEmpty(dataTemplateKey))
                {
                    DataTemplate componentTemplate = element.TryFindResource(dataTemplateKey) as DataTemplate;
                    return componentTemplate;
                }

                string templateKey = "Entity"; //default
                if (cvm.Model is EntityProcessor)
                {
                    templateKey = nameof(EntityProcessor);
                }
                else if (cvm.Model is Entity)
                {
                    templateKey = nameof(Entity);
                }
                else if (cvm.Model is ActorComponent)
                {
                    templateKey = nameof(ActorComponent);
                }
                else if (cvm.Model is DataComponent)
                {
                    templateKey = nameof(DataComponent);
                }
                else if (cvm.Model is ServiceComponent)
                {
                    templateKey = nameof(ServiceComponent);
                }
                DataTemplate defaultTemplate = element.FindResource(templateKey) as DataTemplate;
                return defaultTemplate;
            }
            return null;
        }
    }
}
