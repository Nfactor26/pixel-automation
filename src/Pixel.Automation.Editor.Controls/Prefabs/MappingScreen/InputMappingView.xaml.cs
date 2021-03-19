using System;
using System.Windows.Controls;

namespace Pixel.Automation.Editor.Controls.Prefabs
{
    /// <summary>
    /// Interaction logic for InputMappingView.xaml
    /// </summary>
    public partial class InputMappingView
    {
        public InputMappingView()
        {
            InitializeComponent();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO : causes exception on scrolling. Check why is it even triggered ?
            PropertyMapViewModel rowItem = (sender as ComboBox).DataContext as PropertyMapViewModel;
            rowItem.PropertyMap.AssignFrom = (sender as ComboBox).SelectedItem?.ToString() ?? string.Empty;

        }
    }
}
