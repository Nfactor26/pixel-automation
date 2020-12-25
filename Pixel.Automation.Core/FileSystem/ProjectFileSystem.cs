using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
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
     
        public ProjectFileSystem(ISerializer serializer, ApplicationSettings applicationSettings) : base(serializer, applicationSettings)
        {

        }

        public void Initialize(string projectId, VersionInfo versionInfo)
        {
            this.ActiveVersion = versionInfo;
            this.ProjectId = projectId;
            this.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, projectId, versionInfo.ToString());
            this.TestCaseRepository = Path.Combine(this.WorkingDirectory, "TestCases");
            this.TestDataRepository = Path.Combine(this.WorkingDirectory, "TestDataRepository");
            this.ProjectFile = Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, projectId, $"{projectId}.atm");
            this.ProcessFile = Path.Combine(this.WorkingDirectory, $"{projectId}.proc");
            this.PrefabReferencesFile = Path.Combine(this.WorkingDirectory, "PrefabReferences.ref");

            if (!Directory.Exists(TestCaseRepository))
            {
                Directory.CreateDirectory(TestCaseRepository);
            }
            if (!Directory.Exists(TestDataRepository))
            {
                Directory.CreateDirectory(TestDataRepository);
            }

            base.Initialize();
        }       

        public override void SwitchToVersion(VersionInfo version)
        {
            Initialize(this.ProjectId, version);            
        }

        public ITestCaseFileSystem CreateTestCaseFileSystemFor(string testFixtureId)
        {
            var fileSystem = new TestCaseFileSystem(this.serializer, this.applicationSettings);
            fileSystem.Initialize(this.WorkingDirectory, testFixtureId);
            return fileSystem;
        }

    }
}
