using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pixel.Automation.Core
{
    /// <summary>
    /// Implementation of <see cref="IProjectFileSystem"/>
    /// </summary>
    public class ProjectFileSystem : VersionedFileSystem, IProjectFileSystem
    {
        /// <InheritDoc/>      
        public string ProjectId { get; private set; }

        /// <InheritDoc/>      
        public string ProjectFile { get; private set; }

        /// <InheritDoc/>      
        public string ProcessFile { get; private set; }      

        /// <InheritDoc/>      
        public string TestCaseRepository { get; protected set; }

        /// <InheritDoc/>      
        public string TestDataRepository { get; protected set; }
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="applicationSettings"></param>
        public ProjectFileSystem(ISerializer serializer, ApplicationSettings applicationSettings) 
            : base(serializer, applicationSettings)
        {
         
        }

        /// <InheritDoc/>      
        public void Initialize(AutomationProject automationProject, VersionInfo versionInfo)
        {
            Guard.Argument(automationProject).NotNull();           
            this.Initialize(automationProject.ProjectId, versionInfo);
        }

        /// <InheritDoc/>      
        public override void SwitchToVersion(VersionInfo versionInfo)
        {
            Initialize(this.ProjectId, versionInfo);
        }

        /// <InheritDoc/>      
        void Initialize(string projectId, VersionInfo versionInfo)
        {
            this.ProjectId  = Guard.Argument(projectId).NotNull().NotEmpty();
            this.ActiveVersion = Guard.Argument(versionInfo).NotNull();        
          
            this.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, projectId, versionInfo.ToString());       
            this.ProjectFile = Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, projectId, $"{projectId}.atm");
            this.ProcessFile = Path.Combine(this.WorkingDirectory, Constants.AutomationProcessFileName);        
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
        }

        /// <InheritDoc/>      
        public TestCaseFiles GetTestCaseFiles(TestCase testCase)
        {
            return new TestCaseFiles(testCase, this.TestCaseRepository);
        }

        /// <InheritDoc/>      
        public TestFixtureFiles GetTestFixtureFiles(TestFixture fixture)
        {
            return new TestFixtureFiles(fixture, this.TestCaseRepository);
        }

        /// <InheritDoc/>      
        public IEnumerable<TestDataSource> GetTestDataSources()
        {
            string repositoryFolder = this.TestDataRepository;
            string[] dataSourceFiles = Directory.GetFiles(repositoryFolder, "*.dat");
            foreach (var dataSourceFile in dataSourceFiles)
            {
                var testDataSource = serializer.Deserialize<TestDataSource>(dataSourceFile);
                if(testDataSource.IsDeleted)
                {
                    continue;
                }
                yield return testDataSource;              
            }
            yield break;
        }
    }
}
