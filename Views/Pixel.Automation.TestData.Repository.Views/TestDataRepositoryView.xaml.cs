using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
