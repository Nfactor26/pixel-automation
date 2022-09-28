using Pixel.Automation.Core;
using Pixel.Automation.Editor.Controls.Scripts.EditorUserControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Pixel.Automation.Editor.Controls.Scripts.EditorPropertyGrid
{
    /// <summary>
    /// Interaction logic for BrowseScriptEditor.xaml
    /// </summary>
    public partial class BrowseScriptEditor : UserControl, ITypeEditor
    {
        public static readonly DependencyProperty ActorComponentProperty = DependencyProperty.Register("ActorComponent", typeof(Component), typeof(BrowseScriptEditor),
                                                                                                 new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Component ActorComponent
        {
            get { return (Component)GetValue(ActorComponentProperty); }
            set { SetValue(ActorComponentProperty, value); }
        }

        public static readonly DependencyProperty ShowBrowseButtonProperty = DependencyProperty.Register("ShowBrowseButton", typeof(bool), typeof(BrowseScriptEditor),
                                                                                              new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));
        public bool ShowBrowseButton
        {
            get { return (bool)GetValue(ShowBrowseButtonProperty); }
            set { SetValue(ShowBrowseButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowEditButtonProperty = DependencyProperty.Register("ShowEditButton", typeof(bool), typeof(BrowseScriptEditor),
                                                                                            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.None));
        public bool ShowEditButton
        {
            get { return (bool)GetValue(ShowEditButtonProperty); }
            set { SetValue(ShowEditButtonProperty, value); }
        }


        public BrowseScriptEditor()
        {
            InitializeComponent();
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            this.ActorComponent = propertyItem.Instance as Component;
            var binding = new Binding(propertyItem.PropertyName);
            binding.Source = propertyItem.Instance;           
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            binding.NotifyOnSourceUpdated = true;
            binding.NotifyOnTargetUpdated = true;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(BrowseScriptEditorControl.ScriptPath, TextBox.TextProperty, binding);
            return this;
        }
    }
}
