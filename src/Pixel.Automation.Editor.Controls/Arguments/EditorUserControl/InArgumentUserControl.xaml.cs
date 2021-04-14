using Pixel.Automation.Core.Arguments;
using System.Collections.ObjectModel;
using System.Windows;

namespace Pixel.Automation.Editor.Controls.Arguments
{
    /// <summary>
    /// Interaction logic for InArgumentUserControl.xaml
    /// </summary>
    public partial class InArgumentUserControl : ArgumentUserControl
    {    

        public InArgumentUserControl() : base()
        {
            InitializeComponent();           
            AvailableProperties = new ObservableCollection<string>();
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
                    InitializeScriptName();
                    break;
                case ArgumentMode.Scripted:                   
                    this.Argument.Mode = ArgumentMode.Default;
                    break;
            }            

        }

      
    }
}
