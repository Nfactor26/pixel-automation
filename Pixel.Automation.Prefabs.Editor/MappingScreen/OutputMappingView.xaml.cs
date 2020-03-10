using System.Windows.Controls;

namespace Pixel.Automation.Prefabs.Editor
{
    /// <summary>
    /// Interaction logic for OutputMappingView.xaml
    /// </summary>
    public partial class OutputMappingView
    {
        public OutputMappingView()
        {
            InitializeComponent();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO : causes exception on scrolling. Check why is it even triggered ?
            PropertyMapViewModel rowItem = (sender as ComboBox).DataContext as PropertyMapViewModel;
            rowItem.PropertyMap.AssignTo = (sender as ComboBox).SelectedItem?.ToString() ?? string.Empty;

        }
    }
}
