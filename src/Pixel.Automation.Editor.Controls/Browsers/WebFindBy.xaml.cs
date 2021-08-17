using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Pixel.Automation.Editor.Controls.Browsers
{
    /// <summary>
    /// Interaction logic for WebFindBy.xaml
    /// </summary>
    public partial class WebFindBy : UserControl, ITypeEditor
    {
        public static readonly DependencyProperty FindByProperty = DependencyProperty.Register(
          "FindBy", typeof(string), typeof(WebFindBy),
          new FrameworkPropertyMetadata(default(string)) { BindsTwoWayByDefault = true });

        public string FindBy
        {
            get { return (string)GetValue(FindByProperty); }
            set { SetValue(FindByProperty, value); }
        }

        public List<string> AvailableStrategies { get; } = new List<string>()
        {
            "Id",           
            "Name",
            "CssSelector",
            "ClassName",
            "TagName",
            "XPath",
            "LinkText",
            "PartialLinkText"
        };

        public WebFindBy()
        {
            InitializeComponent();
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            Binding binding = new Binding("Value");
            binding.Source = propertyItem;
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(this, WebFindBy.FindByProperty, binding);
            return this;
        }
    }
}
