using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pixel.Automation.Core
{
    public class ProjectFileSystem : VersionedFileSystem, IProjectFileSystem
    {       
        public string ProjectId { get; private set; }

        public string ProjectFile { get; private set; }
        
        public string ProcessFile { get; private set; }

        public string PrefabReferencesFile { get; private set; }

        public string TestCaseRepository { get; protected set; }

        public string TestDataRepository { get; protected set; }
        
        public ProjectFileSystem(ISerializer serializer, ApplicationSettings applicationSettings) 
            : base(serializer, applicationSettings)
        {
         
        }

        public void Initialize(AutomationProject automationProject, VersionInfo versionInfo)
        {
            Guard.Argument(automationProject).NotNull();           
            this.Initialize(automationProject.ProjectId, versionInfo);
        }

        public override void SwitchToVersion(VersionInfo versionInfo)
        {
            Initialize(this.ProjectId, versionInfo);
        }

        void Initialize(string projectId, VersionInfo versionInfo)
        {
            this.ProjectId  = Guard.Argument(projectId).NotNull().NotEmpty();
            this.ActiveVersion = Guard.Argument(versionInfo).NotNull();        
          
            this.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, projectId, versionInfo.ToString());       
            this.ProjectFile = Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, projectId, $"{projectId}.atm");
            this.ProcessFile = Path.Combine(this.WorkingDirectory, Constants.AutomationProcessFileName);
            this.PrefabReferencesFile = Path.Combine(this.WorkingDirectory, Constants.PrefabRefrencesFileName);
            this.TestCaseRepository = Path.Combine(this.WorkingDirectory, Constants.TestCasesDirectory);
            this.TestDataRepository = Path.Combine(this.WorkingDirectory, Constants.TestDataDirectory);

            if (!Directory.Exists(TestCaseRepository))
            {
                Directory.CreateDirectory(TestCaseRepository);
            }
            if (!Directory.Exists(TestDataRepository))
            {
                Directory.CreateDirectory(TestDataRepository);
            }

            base.Initialize();

            this.ReferenceManager = new AssemblyReferenceManager(this.applicationSettings, this.DataModelDirectory, this.ScriptsDirectory);

        }        

        public ITestCaseFileSystem CreateTestCaseFileSystemFor(string testFixtureId)
        {
            var fileSystem = new TestCaseFileSystem(this.serializer, this.applicationSettings)
            {
                ReferenceManager = this.ReferenceManager
            };
            fileSystem.Initialize(this.WorkingDirectory, testFixtureId);
            return fileSystem;
        }

        public IEnumerable<TestDataSource> GetTestDataSources()
        {
            string repositoryFolder = this.TestDataRepository;
            string[] dataSourceFiles = Directory.GetFiles(repositoryFolder, "*.dat");
            foreach (var dataSourceFile in dataSourceFiles)
            {
                var testDataSource = serializer.Deserialize<TestDataSource>(dataSourceFile);
                yield return testDataSource;              
            }
            yield break;
        }
    }
}
