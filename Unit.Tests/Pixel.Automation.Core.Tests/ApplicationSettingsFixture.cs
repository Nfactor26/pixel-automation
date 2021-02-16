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

            Assert.AreEqual("Applications", applicationSettings.ApplicationDirectory);
            Assert.AreEqual("Automations", applicationSettings.AutomationDirectory);
            Assert.IsTrue(applicationSettings.IsOfflineMode);
            Assert.AreEqual("https://localhost:5001/api", applicationSettings.PersistenceServiceUri);
        }
    }
}
