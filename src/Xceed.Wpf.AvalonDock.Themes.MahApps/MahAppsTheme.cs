using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvalonDock.Themes;

namespace Xceed.Wpf.AvalonDock.Themes
{
    public class MahAppsTheme : Theme
    {
        public override Uri GetResourceUri()
        {
            return new Uri(
                "/Xceed.Wpf.AvalonDock.Themes.MahApps;component/Theme.xaml", 
                UriKind.Relative);  
        }
    }
}
