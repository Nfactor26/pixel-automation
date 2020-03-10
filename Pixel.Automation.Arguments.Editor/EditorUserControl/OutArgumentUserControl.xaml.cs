using Pixel.Automation.Core.Arguments;
using System.Collections.ObjectModel;
using System.Windows;

namespace Pixel.Automation.Arguments.Editor
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
            if (this.OwnerComponent?.EntityManager == null)
                return;
            if (this.Argument.Mode == ArgumentMode.DataBound)
            {
                this.Argument.Mode = ArgumentMode.Scripted;
            }
            else if (this.Argument.Mode == ArgumentMode.Scripted)
            {
                LoadAvailableProperties();
                DeleteScriptFile();
                this.Argument.Mode = ArgumentMode.DataBound;
            }

        }        

    }
}
