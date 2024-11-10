using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.TestDataExplorer.Views
{
    /// <summary>
    /// Interaction logic for TestDataExplorerView.xaml
    /// </summary>
    public partial class TestDataExplorerView : UserControl
    {
        public TestDataExplorerView()
        {
            InitializeComponent();
        }
        private void AddDataSourceClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.ContextMenu.DataContext = button.DataContext;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}
