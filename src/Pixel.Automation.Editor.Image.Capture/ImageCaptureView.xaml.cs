using Caliburn.Micro;
using System.Windows;
using System.Windows.Input;

namespace Pixel.Automation.Editor.Image.Capture
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
