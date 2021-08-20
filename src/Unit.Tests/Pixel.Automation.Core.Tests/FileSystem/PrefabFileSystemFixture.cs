using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.IO;

namespace Pixel.Automation.Core.Tests.FileSystem
{
    [TestFixture]
    class PrefabFileSystemFixture
    {
        private ApplicationSettings appSettings;
        private PrefabFileSystem prefabFileSystem;
        private string prefabId = Guid.NewGuid().ToString();
        private string applicationId = Guid.NewGuid().ToString();
        private string workingDirectory;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {           
            var serializer = Substitute.For<ISerializer>();
            appSettings = new ApplicationSettings() { IsOfflineMode = true, ApplicationDirectory = "Applications", AutomationDirectory = "Automations" };

            prefabFileSystem = new PrefabFileSystem(serializer, appSettings);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, applicationId, "Prefabs", prefabId, "1.0.0.0"));
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, applicationId, "Prefabs", prefabId, "2.0.0.0"));
        }

        [Test]
        [Order(10)]
        public void ValdiateThatFileSystemIsCorrectlyInitializedAndRequiredDirectoriesAreCreated()
        {
            var versionInfo = Substitute.For<VersionInfo>();
            versionInfo.Version = new Version(1, 0);
            prefabFileSystem.Initialize(applicationId, prefabId, versionInfo);

            workingDirectory = Path.Combine(Environment.CurrentDirectory, appSettings.ApplicationDirectory, applicationId, "Prefabs", prefabId, versionInfo.ToString());
            
            Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, appSettings.ApplicationDirectory, applicationId, "Prefabs", prefabId, $"{prefabId}.dat"), prefabFileSystem.PrefabDescriptionFile);
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.PrefabEntityFileName), prefabFileSystem.PrefabFile);
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.PrefabTemplateFileName), prefabFileSystem.TemplateFile);            

        }

        [Test]
        [Order(20)]
        public void ValidateThatPrefabFileSystemCanBeSwitchedToADifferentVersion()
        {
            var versionInfo = Substitute.For<VersionInfo>();
            versionInfo.Version = new Version(2, 0);

            workingDirectory = Path.Combine(Environment.CurrentDirectory, appSettings.ApplicationDirectory, applicationId, "Prefabs", prefabId, versionInfo.ToString());
            prefabFileSystem.SwitchToVersion(versionInfo);

            Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, appSettings.ApplicationDirectory, applicationId, "Prefabs", prefabId, $"{prefabId}.dat"), prefabFileSystem.PrefabDescriptionFile);
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.PrefabEntityFileName), prefabFileSystem.PrefabFile);
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.PrefabTemplateFileName), prefabFileSystem.TemplateFile);
        }
    }
}
