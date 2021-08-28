using Caliburn.Micro;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Core.Docking
{
    public class PanesTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ToolTemplate
        {
            get;
            set;
        }

        public DataTemplate ScreenTemplate
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IAnchorable)
            {
                return ToolTemplate;
            }

            if (item is IScreen)
            {
                return ScreenTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}