using System;
using System.Diagnostics;

namespace Pixel.Automation.Designer.Views
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class ShellView 
    {
        public ShellView()
        {
            InitializeComponent();
        }

        [DebuggerStepThrough]
        private void OnManagerLayoutUpdated(object sender, EventArgs e)
        {
            UpdateFloatingWindows();
        }


        [DebuggerStepThrough]
        public void UpdateFloatingWindows()
        {
            var mainWindow = System.Windows.Window.GetWindow(this);
            var mainWindowIcon = (mainWindow != null) ? mainWindow.Icon : null;
            foreach (var window in Manager.FloatingWindows)
            {
                window.Icon = mainWindowIcon;             
            }
        }
    }
}
