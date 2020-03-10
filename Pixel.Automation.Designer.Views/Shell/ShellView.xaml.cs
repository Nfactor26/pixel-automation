using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

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

        //private void ChangeAccent_Click(object sender, RoutedEventArgs e)
        //{
        //    var theme = MahApps.Metro.ThemeManager.DetectAppStyle(System.Windows.Application.Current);
        //    var accent = MahApps.Metro.ThemeManager.GetAccent("Blue");
        //    MahApps.Metro.ThemeManager.ChangeAppStyle(System.Windows.Application.Current, accent, theme.Item1);
        //}

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
                //window.ShowInTaskbar = showFloatingWindowsInTaskbar;
            }
        }

        //private void ChangeTheme_Click(object sender, RoutedEventArgs e)
        //{
        //    var theme = ThemeManager.DetectAppStyle(Application.Current);
        //    var appTheme = ThemeManager.GetAppTheme("BaseDark");
        //    ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
        //}
    }
}
