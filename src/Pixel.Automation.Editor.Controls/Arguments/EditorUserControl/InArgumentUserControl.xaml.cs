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
            if (this.OwnerComponent?.EntityManager == null || !this.Argument.CanChangeMode)
            {
                return;
            }
            //Set argument mode to next mode based on current mode i.e. rotate from Default -> DataBound -> Scripted -> Default....
            switch (this.Argument.Mode)
            {
                case ArgumentMode.Default:
                    if (this.Argument.AllowedModes.HasFlag(ArgumentMode.DataBound))
                    {
                        this.Argument.Mode = ArgumentMode.DataBound;
                        LoadAvailableProperties();
                    }
                    else if (this.Argument.AllowedModes.HasFlag(ArgumentMode.Scripted))
                    {
                        this.Argument.Mode = ArgumentMode.Scripted;
                        InitializeScriptName();
                    }
                    break;
                case ArgumentMode.DataBound:
                    if (this.Argument.AllowedModes.HasFlag(ArgumentMode.Scripted))
                    {
                        this.Argument.Mode = ArgumentMode.Scripted;
                        InitializeScriptName();
                    }
                    else if (this.Argument.AllowedModes.HasFlag(ArgumentMode.Default))
                    {
                        this.Argument.Mode = ArgumentMode.Default;
                    }
                    break;
                case ArgumentMode.Scripted:
                    if (this.Argument.AllowedModes.HasFlag(ArgumentMode.Default))
                    {
                        this.Argument.Mode = ArgumentMode.Default;
                    }
                    else if (this.Argument.AllowedModes.HasFlag(ArgumentMode.DataBound))
                    {
                        this.Argument.Mode = ArgumentMode.DataBound;
                        LoadAvailableProperties();
                    }
                    break;
            }
        }
        
    }
}
