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
           
            Assert.That(Path.Combine(workingDirectory, Constants.TestCasesDirectory), Is.EqualTo(projectFileSystem.TestCaseRepository));
            Assert.That(Path.Combine(workingDirectory, Constants.TestDataDirectory), Is.EqualTo(projectFileSystem.TestDataRepository));
            Assert.That(Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, projectId, $"{projectId}.atm"), Is.EqualTo(projectFileSystem.ProjectFile));
            Assert.That(Path.Combine(workingDirectory, Constants.AutomationProcessFileName), Is.EqualTo(projectFileSystem.ProcessFile));           

            Assert.That(Directory.Exists(workingDirectory));
            Assert.That(Directory.Exists(projectFileSystem.TestCaseRepository));
            Assert.That(Directory.Exists(projectFileSystem.TestDataRepository));

        }

        [Test]
        [Order(20)]
        public void ValidateThatProjectFileSystemCanBeSwitchedToADifferentVersion()
        {
            var versionInfo = Substitute.For<VersionInfo>();
            versionInfo.Version = new Version(2, 0, 0, 0);

            workingDirectory = Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, projectId, versionInfo.ToString());

            projectFileSystem.SwitchToVersion(versionInfo);
          
            Assert.That(Path.Combine(workingDirectory, Constants.TestCasesDirectory), Is.EqualTo(projectFileSystem.TestCaseRepository));
            Assert.That(Path.Combine(workingDirectory, Constants.TestDataDirectory), Is.EqualTo(projectFileSystem.TestDataRepository));
            Assert.That(Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, projectId, $"{projectId}.atm"), Is.EqualTo(projectFileSystem.ProjectFile));
            Assert.That(Path.Combine(workingDirectory, Constants.AutomationProcessFileName), Is.EqualTo(projectFileSystem.ProcessFile));         

            Assert.That(Directory.Exists(workingDirectory));
            Assert.That(Directory.Exists(projectFileSystem.TestCaseRepository));
            Assert.That(Directory.Exists(projectFileSystem.TestDataRepository));
        }


        /// <summary>
        /// A Test case file system is contained with a parent project file system. Validate that project file system can create a test case file
        /// system given test fixture id.
        /// </summary>
        [Test]
        [Order(30)]
        public void ValidateThatCanGetTestFixtureFilesForAGivenFixture()
        {
            //project file system is using v 2.0 working directory from previous test case.
            var fixture = new Core.TestData.TestFixture() { FixtureId = "f100" };
            var fixtureFiles = projectFileSystem.GetTestFixtureFiles(fixture);
            string fixturesDirectory = Path.Combine(workingDirectory, Constants.TestCasesDirectory, fixture.FixtureId);
            Assert.That(fixtureFiles is not null);
            Assert.That(fixturesDirectory, Is.EqualTo(fixtureFiles.FixtureDirectory));
            Assert.That(Path.Combine(fixturesDirectory, $"{fixture.FixtureId}.fixture"), Is.EqualTo(fixtureFiles.FixtureFile));
            Assert.That(Path.Combine(fixturesDirectory, $"{fixture.FixtureId}.proc"), Is.EqualTo(fixtureFiles.ProcessFile));
            Assert.That(Path.Combine(fixturesDirectory, $"{fixture.FixtureId}.csx"), Is.EqualTo(fixtureFiles.ScriptFile));
        }

        /// <summary>
        /// A Test case file system is contained with a parent project file system. Validate that project file system can create a test case file
        /// system given test fixture id.
        /// </summary>
        [Test]
        [Order(40)]
        public void ValidateThatCanGetTestCaseFilesForAGivenTestCase()
        {
            //project file system is using v 2.0 working directory from previous test case.
            var testCase = new Core.TestData.TestCase() { FixtureId = "f100" , TestCaseId ="t100" };
            var testCaseFiles = projectFileSystem.GetTestCaseFiles(testCase);
            string testCaseDirectory = Path.Combine(workingDirectory, Constants.TestCasesDirectory, testCase.FixtureId, testCase.TestCaseId);
            Assert.That(testCaseFiles is not null);
            Assert.That(testCaseDirectory, Is.EqualTo(testCaseFiles.TestDirectory));
            Assert.That(Path.Combine(testCaseDirectory, $"{testCase.TestCaseId}.test"), Is.EqualTo(testCaseFiles.TestFile));
            Assert.That(Path.Combine(testCaseDirectory, $"{testCase.TestCaseId}.proc"), Is.EqualTo(testCaseFiles.ProcessFile));
            Assert.That(Path.Combine(testCaseDirectory, $"{testCase.TestCaseId}.csx"), Is.EqualTo(testCaseFiles.ScriptFile));
        }
    }
}
