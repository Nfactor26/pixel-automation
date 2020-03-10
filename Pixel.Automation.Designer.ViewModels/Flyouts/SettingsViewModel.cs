using MahApps.Metro;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Pixel.Automation.Designer.ViewModels.Flyouts
{
    public class SettingsViewModel : FlyoutBaseViewModel
    {
        public List<AccentColorMenuData> AccentColors { get; set; }
        public List<AppThemeMenuData> AppThemes { get; set; }

        AccentColorMenuData selectedAccentColor;
        public AccentColorMenuData SelectedAccentColor
        {
            get
            {
                return selectedAccentColor;
            }
            set
            {
                selectedAccentColor = value;
                selectedAccentColor.DoChangeTheme();
            }
        }

        AppThemeMenuData selectedAppTheme;
        public AppThemeMenuData SelectedAppTheme
        {
            get
            {
                return selectedAppTheme;
            }
            set
            {
                selectedAppTheme = value;
                selectedAppTheme.DoChangeTheme();
            }
        }

        public SettingsViewModel()
        {
            this.Header = "Settings";
            this.Position = MahApps.Metro.Controls.Position.Right;
            this.Theme = MahApps.Metro.Controls.FlyoutTheme.Accent;

            // create accent color menu items for the demo
            this.AccentColors = ThemeManager.ColorSchemes
                                            .Select(a => new AccentColorMenuData { Name = a.Name, ColorBrush = a.ShowcaseBrush })
                                            .ToList();

            // create metro theme color menu items for the demo
            this.AppThemes = ThemeManager.Themes
                                         .GroupBy(x => x.BaseColorScheme)
                                         .Select(x => x.First())
                                         .Select(a => new AppThemeMenuData() { Name = a.BaseColorScheme, BorderColorBrush = a.Resources["MahApps.Brushes.ThemeForeground"] as Brush, ColorBrush = a.Resources["MahApps.Brushes.ThemeBackground"] as Brush })
                                         .ToList();
        }
    }

    public class AccentColorMenuData
    {
        public string Name { get; set; }

        public Brush BorderColorBrush { get; set; }

        public Brush ColorBrush { get; set; }

        public AccentColorMenuData()
        {

        }

        public virtual void DoChangeTheme()
        {
            ThemeManager.ChangeThemeColorScheme(Application.Current, this.Name);
        }
    }

    public class AppThemeMenuData : AccentColorMenuData
    {
        public override void DoChangeTheme()
        {
            ThemeManager.ChangeThemeBaseColor(Application.Current, this.Name);
        }
    }
}
