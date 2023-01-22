using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Pixel.Automation.Editor.Controls.Mobile
{
    /// <summary>
    /// Interaction logic for AndroidFindBy.xaml
    /// </summary>
    public partial class AndroidFindBy : UserControl, ITypeEditor
    {
        public static readonly DependencyProperty FindByProperty = DependencyProperty.Register(
          "FindBy", typeof(string), typeof(AndroidFindBy),
          new FrameworkPropertyMetadata(default(string)) { BindsTwoWayByDefault = true });

        public string FindBy
        {
            get { return (string)GetValue(FindByProperty); }
            set { SetValue(FindByProperty, value); }
        }

        public List<string> AvailableStrategies { get; } = new List<string>()
        {
            "Accessibility Id",
            "Android UiAutomator",
            "Android View Tag",
            "Android Data Matcher",
            "Class Name",
            "Id",
            "Name",          
            "XPath"
        };

        public AndroidFindBy()
        {
            InitializeComponent();
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            Binding binding = new Binding("Value");
            binding.Source = propertyItem;
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(this, AndroidFindBy.FindByProperty, binding);
            return this;
        }
    }
}
