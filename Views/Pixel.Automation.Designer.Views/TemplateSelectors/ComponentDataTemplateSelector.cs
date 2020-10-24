using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Processors;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Designer.Views
{
    public class ComponentDataTemplateSelector : DataTemplateSelector
    {
      
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;           
            if (element != null && item != null)
            {
               
                string typeName = item.GetType().Name;
                string dataTemplateKey = TemplateMapper.GetDataTemplateKey(typeName);
                if(!string.IsNullOrEmpty(dataTemplateKey))
                {
                    DataTemplate componentTemplate = element.TryFindResource(dataTemplateKey) as DataTemplate;
                    return componentTemplate;
                }           

                string templateKey = "Entity"; //default
                if (item is EntityProcessor)
                {
                    templateKey = nameof(EntityProcessor);
                }
                else if (item is Entity)
                {
                    templateKey = nameof(Entity);
                }                   
                else if (item is ActorComponent)
                {
                    templateKey = nameof(ActorComponent);
                }
                else if (item is DataComponent)
                {
                    templateKey = nameof(DataComponent);
                }
                else if (item is ServiceComponent)
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
