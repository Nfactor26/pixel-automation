using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Scripting.Editor.Core.Contracts;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Pixel.Automation.Editor.Controls
{
    /// <summary>
    /// Interaction logic for ScriptFileEditor.xaml
    /// </summary>
    public partial class ScriptFileEditor : UserControl, ITypeEditor
    {
        public static readonly DependencyProperty ScriptFileProperty = DependencyProperty.Register("ScriptFile", typeof(string), typeof(ScriptFileEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public string ScriptFile
        {
            get { return (string)GetValue(ScriptFileProperty); }
            set { SetValue(ScriptFileProperty, value); }
        }

        public static readonly DependencyProperty OwnerComponentProperty = DependencyProperty.Register("OwnerComponent", typeof(Component), typeof(ScriptFileEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

        public Component OwnerComponent
        {
            get { return (Component)GetValue(OwnerComponentProperty); }
            set { SetValue(OwnerComponentProperty, value); }
        }

        private string propertyDisplayName;

        public ScriptFileEditor()
        {
            InitializeComponent();
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            this.propertyDisplayName = propertyItem.DisplayName;
            this.OwnerComponent = propertyItem.Instance as Component;
            Binding binding = new Binding("Value");
            binding.Source = propertyItem;
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(this, ScriptFileEditor.ScriptFileProperty, binding);
            return this;
        }

        public async void ShowScriptEditor(object sender, RoutedEventArgs e)
        {
            if (OwnerComponent == null || string.IsNullOrEmpty(ScriptFile))
            {
                return;
            }

            var entityManager = this.OwnerComponent.EntityManager;
            IWindowManager windowManager = entityManager.GetServiceOfType<IWindowManager>();
            IScriptEditorFactory editorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
            await editorFactory.CreateAndShowDialogAsync(windowManager, OwnerComponent, this.ScriptFile, (a) => { return string.Empty; });               
        }
    }
}
