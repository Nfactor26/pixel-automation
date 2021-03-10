using Caliburn.Micro;
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

namespace Pixel.Automation.Editors.Image.Capture
{
    /// <summary>
    /// Interaction logic for ImageCaptureView.xaml
    /// </summary>
    public partial class ImageCaptureView  : Window
    {
        public ImageCaptureView()
        {
            InitializeComponent();           
            
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _ = (this.DataContext as Screen).TryCloseAsync(false);
            }
        }
    }
}
