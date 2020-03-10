using Caliburn.Micro;
using System.Windows;

namespace Pixel.Automation.Prefabs.Editor
{
    /// <summary>
    /// Interaction logic for OutputMappingButton.xaml
    /// </summary>
    public partial class OutputMappingButton : MappingButton
    {        
        public OutputMappingButton()
        {
            InitializeComponent();            
        }

        private async void OpenMappingWindow(object sender, RoutedEventArgs e)
        {
            IWindowManager windowManager = IoC.Get<IWindowManager>();           
            var mappingViewModel = new OutputMappingViewModel(EntityManager, PropertyMapCollection, AssignFrom, AssignTo);
            var result = await windowManager.ShowDialogAsync(mappingViewModel);
            if (result.HasValue && result.Value)
            {
                var configuredMapping = mappingViewModel.GetConfiguredMapping();
                PropertyMapCollection.Clear();
                PropertyMapCollection.AddRange(configuredMapping);
            }

        }
    }
}
