using Caliburn.Micro;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Core.Docking
{
    public class PanesStyleSelector : StyleSelector
	{
		public Style ToolStyle
		{
			get;
			set;
		}

		public Style ScreenStyle
		{
			get;
			set;
		}

		public override Style SelectStyle(object item, DependencyObject container)
		{
            if (item is IToolBox)
            {
                return ToolStyle;
            }

            if (item is IScreen)
            {
                return ScreenStyle;
            }

            return base.SelectStyle(item, container);
		}
	}
}