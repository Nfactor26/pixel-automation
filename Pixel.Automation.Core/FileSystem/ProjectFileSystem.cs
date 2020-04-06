using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.IO;

namespace Pixel.Automation.Core
{
    public class ProjectFileSystem : FileSystem, IProjectFileSystem
    {
        private readonly string automationsDirectory = "Automations";
        private string projectId;

        public string ProjectFile { get; private set; }
        
        public string ProcessFile { get; private set; }

        public string TestCaseDirectory { get; protected set; }

        public string TestDataRepoDirectory { get; protected set; }
     
        public ProjectFileSystem(ISerializer serializer) : base(serializer)
        {

        }

        public void Initialize(string projectId, VersionInfo versionInfo)
        {
            this.ActiveVersion = versionInfo;
            this.projectId = projectId;
            this.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, automationsDirectory, projectId, versionInfo.ToString());
            this.TestCaseDirectory = Path.Combine(this.WorkingDirectory, "TestCases");
            this.TestDataRepoDirectory = Path.Combine(this.WorkingDirectory, "TestDataRepository");
            this.ProjectFile = Path.Combine(Environment.CurrentDirectory, automationsDirectory, projectId, $"{projectId}.atm");
            this.ProcessFile = Path.Combine(this.WorkingDirectory, $"{projectId}.proc");

            if (!Directory.Exists(TestCaseDirectory))
            {
                Directory.CreateDirectory(TestCaseDirectory);
            }
            if (!Directory.Exists(TestDataRepoDirectory))
            {
                Directory.CreateDirectory(TestDataRepoDirectory);
            }

            base.Initialize();
        }

        public void Initialize(string projectId)
        {
            //this.ProjectFile = Path.Combine(Environment.CurrentDirectory, automationsDirectory, projectId, $"{projectId}.atm");
            //AutomationProject automationProject = this.serializer.Deserialize<AutomationProject>(this.ProjectFile);
            //if (automationProject.DeployedVersion == null)
            //{
            //    throw new InvalidOperationException($"There is no deployed version for project : {automationProject.Name}");
            //}
            //Initialize(projectId, automationProject.DeployedVersion);
            throw new NotImplementedException();
        }

        public override void SwitchToVersion(VersionInfo version)
        {
            Initialize(this.projectId, version);            
        }
    }
}
