using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Pixel.Automation.Editor.Controls.Arguments
{
    /// <summary>
    /// Interaction logic for InArgumentEditor.xaml
    /// </summary>
    public partial class InArgumentEditor : ArgumentEditorBase , ITypeEditor
    {
        public InArgumentEditor() : base()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            this.propertyItem = propertyItem;
            this.OwnerComponent = propertyItem.Instance as Component;
            this.Argument = propertyItem.Instance.GetType().GetProperty(propertyItem.PropertyName).GetValue(propertyItem.Instance) as Argument;         
            return this;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.OwnerComponent?.EntityManager == null)
                return;
            LoadAvailableProperties();
        }


        public void ChangeArgumentMode(object sender, RoutedEventArgs e)
        {
            if (this.OwnerComponent?.EntityManager == null)
                return;
            switch (this.Argument.Mode)
            {
                case ArgumentMode.Default:
                    this.Argument.Mode = ArgumentMode.DataBound;
                    LoadAvailableProperties();
                    break;
                case ArgumentMode.DataBound:
                    this.Argument.Mode = ArgumentMode.Scripted;
                    break;
                case ArgumentMode.Scripted:
                    DeleteScriptFile();
                    this.Argument.Mode = ArgumentMode.Default;
                    break;
            }

        }

    }
}
