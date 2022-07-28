using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Pixel.Automation.Editor.Controls.Arguments
{
    /// <summary>
    /// Interaction logic for OutArgumentEditor.xaml
    /// </summary>
    public partial class OutArgumentEditor : ArgumentEditorBase  , ITypeEditor
    {

        public OutArgumentEditor() :base()
        {          
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.OwnerComponent?.EntityManager == null)
            {
                return;
            }
            LoadAvailableProperties();
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            this.propertyItem = propertyItem;
            this.OwnerComponent = propertyItem.Instance as Component;
            this.Argument = propertyItem.Instance.GetType().GetProperty(propertyItem.PropertyName).GetValue(propertyItem.Instance) as Argument;          
            return this;           
        }

        public void ChangeArgumentMode(object sender, RoutedEventArgs e)
        {
            if (this.OwnerComponent?.EntityManager == null || !this.Argument.CanChangeMode)
            {
                return;
            }
            if (this.Argument.Mode == ArgumentMode.DataBound && this.Argument.AllowedModes.HasFlag(ArgumentMode.Scripted))
            {
                this.Argument.Mode = ArgumentMode.Scripted;
                InitializeScriptName();
            }
            else if (this.Argument.Mode == ArgumentMode.Scripted && this.Argument.AllowedModes.HasFlag(ArgumentMode.DataBound))
            {                
                this.Argument.Mode = ArgumentMode.DataBound;
                LoadAvailableProperties();
            }
        }
    }
}
