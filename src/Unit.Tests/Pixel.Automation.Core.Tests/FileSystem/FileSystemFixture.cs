using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Test.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Pixel.Automation.Core.Tests.FileSystem
{
    public class DummyFileSystem : VersionedFileSystem
    {
        private string fileSystemIdentifer;

        public DummyFileSystem(ISerializer serializer, ApplicationSettings applicationSettings, string fileSystemIdentifer) : base(serializer, applicationSettings)
        {
            this.fileSystemIdentifer = fileSystemIdentifer;
        }

        public void Initialize(VersionInfo versionInfo)
        {
            this.ActiveVersion = versionInfo;          
            this.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, fileSystemIdentifer, versionInfo.ToString());
            base.Initialize();
        }

        public override void SwitchToVersion(VersionInfo versionInfo)
        {
            this.Initialize(versionInfo);
        }
    }
    class FileSystemFixture
    {
        private ApplicationSettings appSettings;
        private VersionedFileSystem fileSystem;
        private ISerializer serializer;
        private string fileSystemIdentifer = Guid.NewGuid().ToString();

        [OneTimeSetUp]
        public void OneTimeSetup()
        {           
            serializer = Substitute.For<ISerializer>();     
            appSettings = new ApplicationSettings() { IsOfflineMode = true, ApplicationDirectory = "Applications",  AutomationDirectory = "Automations" };
            fileSystem = new DummyFileSystem(serializer, appSettings, fileSystemIdentifer);
        }


        [TearDown]
        public void Setup()
        {
            serializer.ClearReceivedCalls();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, fileSystemIdentifer, "1.0.0.0"));
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, fileSystemIdentifer, "2.0.0.0"));
        }

        [Test]
        [Order(10)]
        public void ValidateThatFileSystemCanBeCorrectlyInitialized()
        {
            var versionInfo = Substitute.For<VersionInfo>();
            versionInfo.Version = new Version(1, 0);
            string workingdirectory = Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, fileSystemIdentifer, versionInfo.ToString());

            (fileSystem as DummyFileSystem).Initialize(versionInfo);

            Assert.AreEqual(versionInfo.ToString(), fileSystem.ActiveVersion.ToString());
            Assert.AreEqual(workingdirectory, fileSystem.WorkingDirectory);
            Assert.AreEqual(Path.Combine(workingdirectory, "Scripts"), fileSystem.ScriptsDirectory);
            Assert.AreEqual(Path.Combine(workingdirectory, "Temp"), fileSystem.TempDirectory);
            Assert.AreEqual(Path.Combine(workingdirectory, "DataModel"), fileSystem.DataModelDirectory);
            Assert.AreEqual(Path.Combine(workingdirectory, "References"), fileSystem.ReferencesDirectory);
            Assert.AreEqual(Path.Combine(fileSystem.DataModelDirectory, "AssemblyReferences.dat"), fileSystem.ReferencesFile);
            Assert.IsTrue(Directory.Exists(fileSystem.WorkingDirectory));
            Assert.IsTrue(Directory.Exists(fileSystem.ScriptsDirectory));
            Assert.IsTrue(Directory.Exists(fileSystem.DataModelDirectory));
            Assert.IsTrue(Directory.Exists(fileSystem.ReferencesDirectory));          
        }

        [Test]
        [Order(20)]
        public void ValidateThatFileSystemCanBeSwitchToADifferentVersion()
        {
            var versionInfo = Substitute.For<VersionInfo>();
            versionInfo.Version = new Version(2, 0);
            string workingdirectory = Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, fileSystemIdentifer, versionInfo.ToString());

            fileSystem.SwitchToVersion(versionInfo);


            Assert.AreEqual(versionInfo.ToString(), fileSystem.ActiveVersion.ToString());
            Assert.AreEqual(workingdirectory, fileSystem.WorkingDirectory);
            Assert.AreEqual(Path.Combine(workingdirectory, "Scripts"), fileSystem.ScriptsDirectory);
            Assert.AreEqual(Path.Combine(workingdirectory, "Temp"), fileSystem.TempDirectory);
            Assert.AreEqual(Path.Combine(workingdirectory, "DataModel"), fileSystem.DataModelDirectory);
            Assert.AreEqual(Path.Combine(workingdirectory, "References"), fileSystem.ReferencesDirectory);
            Assert.AreEqual(Path.Combine(fileSystem.DataModelDirectory, "AssemblyReferences.dat"), fileSystem.ReferencesFile);
            Assert.IsTrue(Directory.Exists(fileSystem.WorkingDirectory));
            Assert.IsTrue(Directory.Exists(fileSystem.ScriptsDirectory));
            Assert.IsTrue(Directory.Exists(fileSystem.DataModelDirectory));
            Assert.IsTrue(Directory.Exists(fileSystem.ReferencesDirectory));
        }

        [Test]
        [Order(30)]
        public void ValidateThatFileSystemCanProvideAssemblyReferencesWhenAssemblyReferenceFileDoesNotExist()
        {
            var assemblyReferences = fileSystem.GetAssemblyReferences();
            Assert.IsTrue(assemblyReferences.Any());
            serializer.Received(1).Serialize<AssemblyReferences>(Arg.Any<string>(), Arg.Any<AssemblyReferences>());            
        }

        [Test]
        [Order(40)]
        public void ValidateThatFileSystemCanProvideAssemblyReferencesWhenAssemblyReferenceFileExists()
        {
            //create a empty references file and mock serializer to return a new instance of AssemblyReferences
            using (File.Create(fileSystem.ReferencesFile))
            serializer.Deserialize<AssemblyReferences>(Arg.Any<string>()).Returns(new AssemblyReferences());

            var assemblyReferences = fileSystem.GetAssemblyReferences();
            Assert.IsTrue(assemblyReferences.Any());
            serializer.Received(1).Deserialize<AssemblyReferences>(Arg.Any<string>(), Arg.Any<List<Type>>());
        }

        [Test]
        [Order(50)]
        public void ValidateThatAssemblyReferencesCanBeUpdatedUsingFileSystem()
        {
            fileSystem.UpdateAssemblyReferences(new[] { "Assembly.dll" });
            Assert.IsTrue(fileSystem.GetAssemblyReferences().Contains("Assembly.dll"));
            serializer.Received(1).Serialize<AssemblyReferences>(Arg.Any<string>(), Arg.Any<AssemblyReferences>());
        }

        [Test]
        [Order(60)]
        public void ValidateThatFileSystemCanBeUsedToLoadDataFromAFileOfSpecifiedType()
        {
            serializer.Deserialize<Person>(Arg.Any<string>(), Arg.Any<List<Type>>()).Returns(new Person() { Name = "Luffy" });
            var person = fileSystem.LoadFile<Person>("Any valid file path containing serializer person data");
            Assert.AreEqual("Luffy", person.Name);
        }

        [Test]
        [Order(70)]
        public void ValidateThatFileSytemCanBeUsedToLoadAllFilesOfAGivenTypeFromADirectory()
        {
            //Let's create some empty files to be loaded.
            string saveToLocation = Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, fileSystemIdentifer);
            using (File.Create(Path.Combine(saveToLocation, "1.psn"))) { }
            using (File.Create(Path.Combine(saveToLocation, "2.psn"))) { }
            serializer.Deserialize<Person>(Arg.Any<string>(), Arg.Any<List<Type>>()).Returns(new Person() { Name = "Luffy" });

            var persons = fileSystem.LoadFiles<Person>(saveToLocation);
            
            Assert.AreEqual(2, persons.Count());
            serializer.Received(2).Deserialize<Person>(Arg.Any<string>(), Arg.Any<List<Type>>());
        }

        [Test]
        [Order(80)]
        public void ValidateThatFileSystemCanSaveAModelToFile()
        {
            string saveToLocation = Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, fileSystemIdentifer, Guid.NewGuid().ToString());
            var person = new Person() { Name = "Sanji" };
            fileSystem.SaveToFile<Person>(person, saveToLocation);

            serializer.Received(1).Serialize<Person>(Arg.Any<string>(), Arg.Any<Person>(),  Arg.Any<List<Type>>());
        }

        [Test]
        [Order(90)]
        public void ValidateThatFileSystemCanAModelToFileWithGivenFileName()
        {
            string saveToLocation = Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, fileSystemIdentifer, Guid.NewGuid().ToString());
            var person = new Person() { Name = "Sanji" };
            fileSystem.SaveToFile<Person>(person, saveToLocation, "Person.psn");

            serializer.Received(1).Serialize<Person>(Arg.Any<string>(), Arg.Any<Person>(), Arg.Any<List<Type>>());
        }

        [Test]
        [Order(100)]
        public void ValidateThatFileSystemCanBeUsedToCreateANewFileWithGivenContent()
        {          
            string saveToLocation = Path.Combine(Environment.CurrentDirectory, appSettings.AutomationDirectory, fileSystemIdentifer, Guid.NewGuid().ToString());
            Directory.CreateDirectory(saveToLocation);
            fileSystem.CreateOrReplaceFile(saveToLocation, "1.txt", "Hello World!!");          
            Assert.IsTrue(File.Exists(Path.Combine(saveToLocation, "1.txt")));
            Assert.AreEqual("Hello World!!", File.ReadAllText(Path.Combine(saveToLocation, "1.txt")));

            //This time existing file should be deleted first and then file should be created with new content
            fileSystem.CreateOrReplaceFile(saveToLocation, "1.txt", "Hello Again!!");
            Assert.IsTrue(File.Exists(Path.Combine(saveToLocation, "1.txt")));
            Assert.AreEqual("Hello Again!!", File.ReadAllText(Path.Combine(saveToLocation, "1.txt")));

        }

        [Test]
        [Order(110)]
        public void ValidateThatRelativePathToWorkingDirectoryCanBeRetrieved()
        {
            var absolutePath = Path.Combine(fileSystem.WorkingDirectory, "A", "B");
            var relativePath = fileSystem.GetRelativePath(absolutePath);
            Assert.AreEqual("A\\B", relativePath);
        }
    }
}
