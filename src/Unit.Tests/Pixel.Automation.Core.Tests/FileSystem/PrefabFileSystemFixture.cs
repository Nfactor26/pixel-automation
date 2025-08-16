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
            appSettings = new ApplicationSettings() 
            { 
                IsOfflineMode = true, 
                ApplicationDirectory = "Applications", 
                AutomationDirectory = "Automations"
            };

            prefabFileSystem = new PrefabFileSystem(serializer, appSettings);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, appSettings.ApplicationDirectory, applicationId), true);         
        }

        [Test]
        [Order(10)]
        public void ValdiateThatFileSystemIsCorrectlyInitializedAndRequiredDirectoriesAreCreated()
        {
            var versionInfo = Substitute.For<VersionInfo>();
            versionInfo.Version = new Version(1, 0);
            var prefabProject = new PrefabProject() { ApplicationId = applicationId, ProjectId = prefabId };
            prefabFileSystem.Initialize(prefabProject, versionInfo);

            workingDirectory = Path.Combine(Environment.CurrentDirectory, appSettings.ApplicationDirectory, applicationId, Constants.PrefabsDirectory, prefabId, versionInfo.ToString());
            
            Assert.That(Path.Combine(Environment.CurrentDirectory, appSettings.ApplicationDirectory, applicationId, Constants.PrefabsDirectory, prefabId, $"{prefabId}.atm"), Is.EqualTo(prefabFileSystem.PrefabDescriptionFile));
            Assert.That(Path.Combine(workingDirectory, Constants.PrefabProcessFileName), Is.EqualTo(prefabFileSystem.PrefabFile));
            Assert.That(Path.Combine(workingDirectory, Constants.PrefabTemplateFileName), Is.EqualTo(prefabFileSystem.TemplateFile));            

        }

        [Test]
        [Order(20)]
        public void ValidateThatPrefabFileSystemCanBeSwitchedToADifferentVersion()
        {
            var versionInfo = Substitute.For<VersionInfo>();
            versionInfo.Version = new Version(2, 0);

            workingDirectory = Path.Combine(Environment.CurrentDirectory, appSettings.ApplicationDirectory, applicationId, Constants.PrefabsDirectory, prefabId, versionInfo.ToString());
            prefabFileSystem.SwitchToVersion(versionInfo);

            Assert.That(Path.Combine(Environment.CurrentDirectory, appSettings.ApplicationDirectory, applicationId, Constants.PrefabsDirectory, prefabId, $"{prefabId}.atm"), Is.EqualTo(prefabFileSystem.PrefabDescriptionFile));
            Assert.That(Path.Combine(workingDirectory, Constants.PrefabProcessFileName), Is.EqualTo(prefabFileSystem.PrefabFile));
            Assert.That(Path.Combine(workingDirectory, Constants.PrefabTemplateFileName), Is.EqualTo(prefabFileSystem.TemplateFile));
        }
    }
}
