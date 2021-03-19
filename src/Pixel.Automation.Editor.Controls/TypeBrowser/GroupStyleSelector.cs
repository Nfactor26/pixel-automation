using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Pixel.Automation.Editor.TypeBrowser
{
    public class GroupStyleSelector : StyleSelector
    {
        public Style NoGroupHeaderStyle { get; set; }

        public Style DefaultGroupStyle { get; set; }

        public Style ChildGroupStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var group = item as CollectionViewGroup;
            if (group == null)
            {
                return DefaultGroupStyle;
            }

            if (string.IsNullOrEmpty(group.Name?.ToString()))
            {
                return NoGroupHeaderStyle;
            }

            if (!group.IsBottomLevel)
            {
                return DefaultGroupStyle;
            }
            else
            {
                return ChildGroupStyle;
            }
        }
    }
}
