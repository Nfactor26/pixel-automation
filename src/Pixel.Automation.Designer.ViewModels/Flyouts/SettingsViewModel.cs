using Dawn;
using Microsoft.Extensions.Configuration;
using Pixel.Automation.Core;
using Pixel.Automation.Editor.Core;
using System;
using System.IO;

namespace Pixel.Automation.Designer.ViewModels.Flyouts
{
    public class SettingsViewModel : FlyoutBaseViewModel
    {
        private readonly IConfiguration configurationManager;

        public ThemeSettings ThemeSettings { get; private set; }

        public ApplicationSettings ApplicationSettings { get; private set; }
       
        public SettingsViewModel(IConfiguration configurationManager, ApplicationSettings applicationSettings, UserSettings userSettings)
        {
            this.configurationManager = Guard.Argument(configurationManager, nameof(configurationManager)).NotNull().Value;

            this.Header = "Settings";
            this.Position = MahApps.Metro.Controls.Position.Right;
            this.Theme = MahApps.Metro.Controls.FlyoutTheme.Inverse;

            this.ApplicationSettings = applicationSettings;
            this.ThemeSettings = new ThemeSettings(userSettings);
        }


        public void Save()
        {
            var userSettings = new UserSettings()
            {
                Theme = this.ThemeSettings.SelectedAppTheme?.Name ?? "Light",
                Accent = this.ThemeSettings.SelectedAccentColor?.Name ?? "Crimson"
            };  
          
            AddOrUpdateSection("userSettings", userSettings);
            AddOrUpdateSection("applicationSettings", this.ApplicationSettings);
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
}
