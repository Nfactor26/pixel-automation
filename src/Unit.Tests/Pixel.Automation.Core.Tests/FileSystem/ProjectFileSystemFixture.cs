using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.IO;

namespace Pixel.Automation.Core.Tests.FileSystem
{
    class ProjectFileSystemFixture
    {
        private ApplicationSettings appSettings;    
        private ProjectFileSystem projectFileSystem;
        private string projectId = "Project-Id";
        private string workingDirectory;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var serializer = Substitute.For<ISerializer>();         
            appSettings = new ApplicationSettings() { IsOfflineMode = true, ApplicationDirectory = "Applications", AutomationDirectory = "Automations" };      
           
            projectFileSystem = new ProjectFileSystem(serializer, appSettings);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, projectId), true);           
        }

        [Test]
        [Order(10)]
        public void ValdiateThatFileSystemIsCorrectlyInitializedAndRequiredDirectoriesAreCreated()
        {
            var versionInfo = Substitute.For<VersionInfo>();
            versionInfo.Version = new Version(1, 0, 0, 0);
            var project = new AutomationProject() { ProjectId = projectId };
            projectFileSystem.Initialize(project, versionInfo);

            workingDirectory = Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, projectId, versionInfo.ToString());
           
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.TestCasesDirectory), projectFileSystem.TestCaseRepository);
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.TestDataDirectory), projectFileSystem.TestDataRepository);
            Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, projectId, $"{projectId}.atm"), projectFileSystem.ProjectFile);
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.AutomationProcessFileName), projectFileSystem.ProcessFile);
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.PrefabRefrencesFileName), projectFileSystem.PrefabReferencesFile);

            Assert.IsTrue(Directory.Exists(workingDirectory));
            Assert.IsTrue(Directory.Exists(projectFileSystem.TestCaseRepository));
            Assert.IsTrue(Directory.Exists(projectFileSystem.TestDataRepository));

        }

        [Test]
        [Order(20)]
        public void ValidateThatProjectFileSystemCanBeSwitchedToADifferentVersion()
        {
            var versionInfo = Substitute.For<VersionInfo>();
            versionInfo.Version = new Version(2, 0, 0, 0);

            workingDirectory = Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, projectId, versionInfo.ToString());

            projectFileSystem.SwitchToVersion(versionInfo);
          
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.TestCasesDirectory), projectFileSystem.TestCaseRepository);
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.TestDataDirectory), projectFileSystem.TestDataRepository);
            Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, projectId, $"{projectId}.atm"), projectFileSystem.ProjectFile);
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.AutomationProcessFileName), projectFileSystem.ProcessFile);
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.PrefabRefrencesFileName), projectFileSystem.PrefabReferencesFile);

            Assert.IsTrue(Directory.Exists(workingDirectory));
            Assert.IsTrue(Directory.Exists(projectFileSystem.TestCaseRepository));
            Assert.IsTrue(Directory.Exists(projectFileSystem.TestDataRepository));
        }


        /// <summary>
        /// A Test case file system is contained with a parent project file system. Validate that project file system can create a test case file
        /// system given test fixture id.
        /// </summary>
        [Test]
        [Order(30)]
        public void ProjectFileSystemCanCreateTestCaseFileSystem()
        {
            //project file system is using v 2.0 working directory from previous test case.
            string fixtureId = "Fixture - Id";
            var testCaseFileSystem = projectFileSystem.CreateTestCaseFileSystemFor(fixtureId);
            Assert.IsNotNull(testCaseFileSystem);
            Assert.AreEqual(Path.Combine(workingDirectory, Constants.TestCasesDirectory, fixtureId), testCaseFileSystem.FixtureDirectory);
        }
    }
}
