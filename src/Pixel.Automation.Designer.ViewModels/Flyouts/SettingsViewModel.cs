using ControlzEx.Theming;
using Dawn;
using MahApps.Metro;
using Microsoft.Extensions.Configuration;
using Pixel.Automation.Core;
using Pixel.Automation.Editor.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Pixel.Automation.Designer.ViewModels.Flyouts
{
    public class SettingsViewModel : FlyoutBaseViewModel
    {
        private readonly IConfiguration configurationManager;

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

        private string persistenceServiceUri;
        public string PersistenceServiceUri
        {
            get => this.persistenceServiceUri;
            set
            {
                this.persistenceServiceUri = value;
                if(string.IsNullOrEmpty(value))
                {
                    this.IsOfflineMode = true;
                }
                NotifyOfPropertyChange();
            }
        }

        private bool isOfflineMode;
        public bool IsOfflineMode
        {
            get => this.isOfflineMode;
            set
            {
                this.isOfflineMode = value;
                NotifyOfPropertyChange();
            }
        }
       

        public SettingsViewModel(IConfiguration configurationManager)
        {
            this.configurationManager = Guard.Argument(configurationManager, nameof(configurationManager)).NotNull().Value;

            this.Header = "Settings";
            this.Position = MahApps.Metro.Controls.Position.Right;
            this.Theme = MahApps.Metro.Controls.FlyoutTheme.Inverse;


            // create accent color menu items for the demo
            this.AccentColors = ThemeManager.Current.ColorSchemes.Select(a => new AccentColorMenuData { Name = a })
                                            .ToList();

            // create metro theme color menu items for the demo
            this.AppThemes = ThemeManager.Current.Themes
                                         .GroupBy(x => x.BaseColorScheme)
                                         .Select(x => x.First())
                                         .Select(a => new AppThemeMenuData() { Name = a.BaseColorScheme })
                                         .ToList();

            var userSettings = configurationManager.GetSection("userSettings").Get<UserSettings>();
            this.SelectedAccentColor = this.AccentColors.FirstOrDefault(a => a.Name.Equals(userSettings.Accent));
            this.SelectedAppTheme = this.AppThemes.FirstOrDefault(a => a.Name.Equals(userSettings.Theme));

            var applicationSettings = configurationManager.GetSection("applicationSettings").Get<ApplicationSettings>();
            this.PersistenceServiceUri = applicationSettings.PersistenceServiceUri;
            this.IsOfflineMode = applicationSettings.IsOfflineMode;
        }


        public void Save()
        {
            var userSettings = configurationManager.GetSection("userSettings").Get<UserSettings>();
            userSettings.Theme = this.selectedAppTheme?.Name ?? "Light";
            userSettings.Accent = this.selectedAccentColor?.Name ?? "Blue";
        
            var applicationSettings = configurationManager.GetSection("applicationSettings").Get<ApplicationSettings>();
            applicationSettings.PersistenceServiceUri = this.persistenceServiceUri;
            applicationSettings.IsOfflineMode = this.isOfflineMode;

            AddOrUpdateSection("userSettings", userSettings);
            AddOrUpdateSection("applicationSettings", applicationSettings);
        }

        private void AddOrUpdateSection<T>(string sectionKey, T sectionData)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, "appsettings.json");
            string json = File.ReadAllText(filePath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            jsonObj[sectionKey] = Newtonsoft.Json.Linq.JObject.FromObject(sectionData);
            string updatedContent = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, updatedContent);
        }


    }

    public class AccentColorMenuData
    {
        public string Name { get; set; }

        //public Brush BorderColorBrush { get; set; }

        //public Brush ColorBrush { get; set; }

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
}
