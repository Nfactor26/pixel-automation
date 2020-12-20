using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    public class ProjectVersionViewModel : PropertyChangedBase
    {
        private readonly ILogger logger = Log.ForContext<ProjectVersionViewModel>();

        private readonly AutomationProject automationProject;
        private readonly IProjectFileSystem fileSystem;

        public ProjectVersion ProjectVersion { get; }

        public Version Version
        {
            get => ProjectVersion.Version;
            set => ProjectVersion.Version = value;
        }

        public bool IsDeployed
        {
            get => ProjectVersion.IsDeployed;
            set
            {
                if (!ProjectVersion.IsDeployed && value)
                {
                    ProjectVersion.IsDeployed = value;
                }
                NotifyOfPropertyChange(() => IsDeployed);
            }
        }

        public string DataModelAssembly
        {
            get => ProjectVersion.DataModelAssembly;
            private set
            {
                if (ProjectVersion.IsDeployed && !string.IsNullOrEmpty(value))
                {
                    ProjectVersion.DataModelAssembly = value;
                }
                NotifyOfPropertyChange(() => DataModelAssembly);
            }
        }

        /// <summary>
        /// Indicates if this version is the active version. Active version open for edit by default.
        /// </summary>
        public bool IsActive
        {
            get => ProjectVersion.IsActive;
            set
            {
                ProjectVersion.IsActive = value;
                NotifyOfPropertyChange(() => IsActive);
            }
        }
   

        public ProjectVersionViewModel(AutomationProject automationProject, ProjectVersion projectVersion, IProjectFileSystem fileSystem)
        {
            this.automationProject = Guard.Argument(automationProject, nameof(automationProject)).NotNull();
            this.ProjectVersion = Guard.Argument(projectVersion, nameof(projectVersion)).NotNull();
            this.fileSystem = Guard.Argument(fileSystem).NotNull().Value;
        }


        public ProjectVersion Clone()
        {
            logger.Information($"Trying to clone version : {this.ProjectVersion} of project: {this.automationProject.Name}");
            ////Increment active version for project        
            ProjectVersion newVersionInfo = new ProjectVersion(new Version(this.ProjectVersion.Version.Major + 1, 0, 0, 0))
            {
                IsActive = true,
                IsDeployed = false
            };

            this.fileSystem.Initialize(this.automationProject.Name, this.ProjectVersion);
            var currentWorkingDirectory = new DirectoryInfo(this.fileSystem.WorkingDirectory);
            var newWorkingDirectory = Path.Combine(currentWorkingDirectory.Parent.FullName, newVersionInfo.ToString());
            Directory.CreateDirectory(newWorkingDirectory);

            ////copy contents from previous version directory to new version directory
            CopyAll(currentWorkingDirectory, new DirectoryInfo(newWorkingDirectory));
         
            void CopyAll(DirectoryInfo source, DirectoryInfo target)
            {
                // Copy each file into the new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }

                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir);
                }
            }
            logger.Information($"Completed cloning version : {this.ProjectVersion} of project: {this.automationProject.Name}. Cloned version is : {newVersionInfo}");
            return newVersionInfo;
        }


        /// <summary>
        /// Copy last compiled dll from temp folder to References folder.
        /// Set IsDeployed to true and set the assembly name
        /// </summary>
        public void Deploy(IWorkspaceManagerFactory workspaceFactory)
        {          

            this.fileSystem.Initialize(this.automationProject.Name, this.ProjectVersion);
            logger.Information($"Project file system has been initialized.");

            ICodeWorkspaceManager workspaceManager = workspaceFactory.CreateCodeWorkspaceManager(this.fileSystem.DataModelDirectory);
            workspaceManager.WithAssemblyReferences(this.fileSystem.GetAssemblyReferences());
            workspaceManager.AddProject(this.automationProject.Name, $"Pixel.Automation.{this.automationProject.GetProjectName()}",  Array.Empty<string>());
            string[] existingDataModelFiles = Directory.GetFiles(this.fileSystem.DataModelDirectory, "*.cs");
            if (existingDataModelFiles.Any())
            {
                //This will add all the documents to workspace so that they are available during compilation
                foreach (var dataModelFile in existingDataModelFiles)
                {
                    string documentName = Path.GetFileName(dataModelFile);
                    if (!workspaceManager.HasDocument(documentName, this.automationProject.Name))
                    {
                        workspaceManager.AddDocument(documentName, this.automationProject.Name, File.ReadAllText(dataModelFile));
                    }
                }
            }

            string assemblyName = this.automationProject.Name.Trim().Replace(' ', '_');
            using (var compilationResult = workspaceManager.CompileProject(this.automationProject.Name, assemblyName))
            {
                compilationResult.SaveAssemblyToDisk(this.fileSystem.ReferencesDirectory);            
            }

            logger.Information($"Workspace prepared and compilation done.");

            this.IsDeployed = true;
            this.IsActive = false;
            this.DataModelAssembly = $"{assemblyName}.dll";

            logger.Information($"Data model assembly name is : {this.DataModelAssembly}");

            //Replace the assemly name in the process file
            UpdateAssemblyReference(this.fileSystem.ProcessFile, assemblyName);

            logger.Information($"Assembly references updated in process file.");

            //Replace the assembly name in all of the test process file
            foreach (var directory in Directory.GetDirectories(this.fileSystem.TestCaseRepository))
            {
                var testProcessFile = Directory.GetFiles(directory, "*.proc").FirstOrDefault();
                if(testProcessFile != null)
                {
                    UpdateAssemblyReference(testProcessFile, assemblyName);
                }
            }

            logger.Information($"Assembly references updated in all tes cases.");
        }

        /// <summary>
        /// At design time, assembly names are generated with name_{digit}.dll syntax. Replace this in process file with specified assembly name.
        /// </summary>
        /// <param name="processFile"></param>
        /// <param name="assemblyName"></param>
        private void UpdateAssemblyReference(string processFile, string assemblyName)
        {
            string fileContents = File.ReadAllText(processFile);
            Regex regex = new Regex($"({assemblyName})(_\\d+)");
            fileContents = regex.Replace(fileContents, (m) =>
            {
                return assemblyName;
            });
            File.WriteAllText(processFile, fileContents);

        }

    }
}
