using Caliburn.Micro;
using ControlzEx.Theming;
using Pixel.Automation.Editor.Core;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.Designer.ViewModels.Flyouts
{
    public class AccentColorMenuData
    {
        public string Name { get; set; }
    
        public AccentColorMenuData()
        {

        }

        public virtual void DoChangeTheme()
        {
            ThemeManager.Current.ChangeThemeColorScheme(System.Windows.Application.Current, this.Name);
        }
    }

    public class AppThemeMenuData : AccentColorMenuData
    {
        public override void DoChangeTheme()
        {
            ThemeManager.Current.ChangeThemeBaseColor(System.Windows.Application.Current, this.Name);
        }
    }

    public class ThemeSettings : PropertyChangedBase
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
                this.selectedAccentColor = value;
                this.selectedAccentColor.DoChangeTheme();
                NotifyOfPropertyChange();
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
                this.selectedAppTheme = value;
                this.selectedAppTheme.DoChangeTheme();
                NotifyOfPropertyChange();
            }
        }

        public ThemeSettings(UserSettings userSettings)
        {

            this.AccentColors = ThemeManager.Current.ColorSchemes.Select(a => new AccentColorMenuData { Name = a })
                                         .ToList();

            // create metro theme color menu items for the demo
            this.AppThemes = ThemeManager.Current.Themes
                                         .GroupBy(x => x.BaseColorScheme)
                                         .Select(x => x.First())
                                         .Select(a => new AppThemeMenuData() { Name = a.BaseColorScheme })
                                         .ToList();
            this.SelectedAccentColor = this.AccentColors.FirstOrDefault(a => a.Name.Equals(userSettings.Accent));
            this.SelectedAppTheme = this.AppThemes.FirstOrDefault(a => a.Name.Equals(userSettings.Theme));
        }

    }
}
