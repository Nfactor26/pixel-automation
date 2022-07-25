using Pixel.Automation.Core.Arguments;
using System.Collections.ObjectModel;
using System.Windows;

namespace Pixel.Automation.Editor.Controls.Arguments
{
    /// <summary>
    /// Interaction logic for ArgumentUserControl.xaml
    /// </summary>
    public partial class OutArgumentUserControl : ArgumentUserControl
    {
        public OutArgumentUserControl() :base()
        {
            InitializeComponent();
            AvailableProperties = new ObservableCollection<string>();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.OwnerComponent?.EntityManager == null)
                return;
            LoadAvailableProperties();
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
