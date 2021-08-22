using System.Windows;
using System.Windows.Controls;

namespace Pixel.Automation.TestData.Repository.Views
{
    /// <summary>
    /// Interaction logic for TestDataRepositoryView.xaml
    /// </summary>
    public partial class TestDataRepositoryView : UserControl
    {
        public TestDataRepositoryView()
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
