using Caliburn.Micro;
using Pixel.Automation.Editor.Core;
using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.Designer.ViewModels
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
                return ToolStyle;

            if (item is IScreen)
                return ScreenStyle;

            return base.SelectStyle(item, container);
		}
	}
}