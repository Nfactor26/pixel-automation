using NUnit.Framework;

namespace Pixel.Automation.Core.Tests
{
    class ApplicationSettingsFixture
    {
        [Test]
        public void ValidateThatApplicationSettingsCanBeInitialized()
        {
            var applicationSettings = new ApplicationSettings()
            {
                ApplicationDirectory = "Applications",
                AutomationDirectory = "Automations",
                IsOfflineMode = true,
                PersistenceServiceUri = "https://localhost:5001/api"
            };

            Assert.That(applicationSettings.ApplicationDirectory, Is.EqualTo("Applications"));
            Assert.That(applicationSettings.AutomationDirectory, Is.EqualTo("Automations"));
            Assert.That(applicationSettings.IsOfflineMode);
            Assert.That(applicationSettings.PersistenceServiceUri, Is.EqualTo("https://localhost:5001/api"));
        }
    }
}
