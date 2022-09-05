using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using System;
using System.IO;

namespace Pixel.Automation.Core.Tests.FileSystem
{
    class TestCaseFileSystemFixture
    {
        private ApplicationSettings appSettings;
        private TestCaseFileSystem testCaseFileSystem;
        private string testFixtureId = Guid.NewGuid().ToString();

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var serializer = Substitute.For<ISerializer>();
            appSettings = new ApplicationSettings() { IsOfflineMode = true, ApplicationDirectory = "Applications", AutomationDirectory = "Automations" };
            testCaseFileSystem = new TestCaseFileSystem();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, Constants.TestCasesDirectory));          
        }


        [Test]
        [Order(10)]
        public void ValidateThatTestCaseFileSystemCanBeCorrectlyInitialized()
        {
            testCaseFileSystem.Initialize(Environment.CurrentDirectory, testFixtureId);

            Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, Constants.TestCasesDirectory, testFixtureId), testCaseFileSystem.FixtureDirectory);
            Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, Constants.TestCasesDirectory, testFixtureId, $"{testFixtureId}.fixture"), testCaseFileSystem.FixtureFile);
            Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, Constants.TestCasesDirectory, testFixtureId, $"{testFixtureId}.proc"), testCaseFileSystem.FixtureProcessFile);
            Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, Constants.TestCasesDirectory, testFixtureId, $"{testFixtureId}.csx"), testCaseFileSystem.FixtureScript);
            Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, Constants.TestCasesDirectory, testFixtureId, "1.test"), testCaseFileSystem.GetTestCaseFile("1"));
            Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, Constants.TestCasesDirectory, testFixtureId, "1.proc"), testCaseFileSystem.GetTestProcessFile("1"));
            Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, Constants.TestCasesDirectory, testFixtureId, "1.csx"), testCaseFileSystem.GetTestScriptFile("1"));
            Assert.IsTrue(Directory.Exists(testCaseFileSystem.FixtureDirectory));
        }

        [Test]
        [Order(20)]
        public void ValidateThatTestCaseFileSystemCanDeleteFilesAssociatedWithAGivenTestCase()
        {
            //create the test case files first which would usually be created when adding a new test case
            using (File.Create(testCaseFileSystem.GetTestCaseFile("1")))
            using (File.Create(testCaseFileSystem.GetTestProcessFile("1")))
            using (File.Create(testCaseFileSystem.GetTestScriptFile("1")))

            Assert.IsTrue(File.Exists(testCaseFileSystem.GetTestCaseFile("1")));
            Assert.IsTrue(File.Exists(testCaseFileSystem.GetTestProcessFile("1")));
            Assert.IsTrue(File.Exists(testCaseFileSystem.GetTestScriptFile("1")));

            //Ask the test case file system to delete files associated with a given test case
            testCaseFileSystem.DeleteTestCase("1");

            Assert.IsFalse(File.Exists(testCaseFileSystem.GetTestCaseFile("1")));
            Assert.IsFalse(File.Exists(testCaseFileSystem.GetTestProcessFile("1")));
            Assert.IsFalse(File.Exists(testCaseFileSystem.GetTestScriptFile("1")));
        }
    }
}
