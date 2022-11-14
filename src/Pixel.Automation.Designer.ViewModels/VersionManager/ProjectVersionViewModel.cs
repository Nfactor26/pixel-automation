using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Reference.Manager.Contracts;
using Serilog;
using System.IO;
using System.Text.RegularExpressions;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    public class ProjectVersionViewModel : PropertyChangedBase
    {
        private readonly ILogger logger = Log.ForContext<ProjectVersionViewModel>();

        private readonly AutomationProject automationProject;
        private readonly IProjectFileSystem fileSystem;
        private readonly Lazy<IReferenceManager> referenceManager;

        public ProjectVersion ProjectVersion { get; }

        public Version Version
        {
            get => ProjectVersion.Version;
            set => ProjectVersion.Version = value;
        }

        public bool IsPublished
        {
            get => ProjectVersion.IsPublished;            
        }

        /// <summary>
        /// Indicates if this version is the active version. Active version open for edit by default.
        /// </summary>
        public bool IsActive
        {
            get => ProjectVersion.IsActive;
            set
            {
                //can be only set to false when version is active
                if (ProjectVersion.IsActive && !value)
                {
                    ProjectVersion.IsActive = value;
                }
                NotifyOfPropertyChange(() => IsActive);
                NotifyOfPropertyChange(() => IsPublished);               
            }
        }

        public bool CanPublish { get; private set; }
        
        public string DataModelAssembly
        {
            get => ProjectVersion.DataModelAssembly;
            private set
            {
                if (ProjectVersion.IsPublished && !string.IsNullOrEmpty(value))
                {
                    ProjectVersion.DataModelAssembly = value;
                }
                NotifyOfPropertyChange(() => DataModelAssembly);
            }
        }


        public ProjectVersionViewModel(AutomationProject automationProject, ProjectVersion projectVersion, IProjectFileSystem fileSystem, IReferenceManagerFactory referenceManagerFactory)
        {
            this.automationProject = Guard.Argument(automationProject, nameof(automationProject)).NotNull();
            this.ProjectVersion = Guard.Argument(projectVersion, nameof(projectVersion)).NotNull();
            this.fileSystem = Guard.Argument(fileSystem).NotNull().Value;
            this.referenceManager = new Lazy<IReferenceManager>(() => { return referenceManagerFactory.CreateReferenceManager(this.automationProject.ProjectId, this.ProjectVersion.ToString(), this.fileSystem); });
            this.CanPublish = !projectVersion.IsPublished && !projectVersion.Version.Equals(automationProject.LatestActiveVersion.Version);
        }


        public async Task<ProjectVersion> CloneAsync(IProjectDataManager projectDataManager)
        {            
            ProjectVersion newVersion;
            if (!this.automationProject.AvailableVersions.Any(a => a.Version.Major.Equals(this.ProjectVersion.Version.Major + 1)))
            {
                newVersion = new ProjectVersion(new Version(this.ProjectVersion.Version.Major + 1, 0, 0, 0))
                {
                    IsActive = true
                };
            }
            else
            {
                var versionsWithSameMajor = this.automationProject.AvailableVersions.Select(s => s.Version).Where(v => v.Major.Equals(this.ProjectVersion.Version.Major));
                int nextMinor = versionsWithSameMajor.Select(v => v.Minor).Max() + 1;
                newVersion = new ProjectVersion(new Version(this.ProjectVersion.Version.Major, nextMinor, 0, 0))
                {
                    IsActive = true
                };
            }
            await projectDataManager.AddProjectVersionAsync(this.automationProject, newVersion, this.ProjectVersion);
            return newVersion;
        }


        /// <summary>
        /// Copy last compiled dll from temp folder to References folder.
        /// Set IsActive to false and set the assembly name
        /// </summary>
        public async Task PublishAsync(IWorkspaceManagerFactory workspaceFactory, IProjectDataManager projectDataManager)
        {          
            if(!this.IsPublished)
            {
                this.fileSystem.Initialize(this.automationProject, this.ProjectVersion);
                logger.Information($"Project file system has been initialized.");

                ICodeWorkspaceManager workspaceManager = workspaceFactory.CreateCodeWorkspaceManager(this.fileSystem.DataModelDirectory);
                workspaceManager.WithAssemblyReferences(this.referenceManager.Value.GetCodeEditorReferences());
                workspaceManager.AddProject(this.automationProject.Name, this.automationProject.Namespace, Array.Empty<string>());
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

                string assemblyName = this.automationProject.Namespace;
                using (var compilationResult = workspaceManager.CompileProject(this.automationProject.Name, assemblyName))
                {
                    compilationResult.SaveAssemblyToDisk(this.fileSystem.ReferencesDirectory);
                }

                logger.Information($"Workspace prepared and compilation done.");

                this.IsActive = false;
                this.CanPublish = false;
                this.DataModelAssembly = $"{assemblyName}.dll";

                logger.Information($"Data model assembly name is : {this.DataModelAssembly}");

                //save the datamodel assembly file
                string dataModelAssemblyFile = Path.Combine(this.fileSystem.ReferencesDirectory, this.DataModelAssembly);
                await projectDataManager.AddDataFileAsync(automationProject, this.ProjectVersion, dataModelAssemblyFile, automationProject.ProjectId);
              
                //Replace the assemly name in the process file and save process file
                if(UpdateAssemblyReference(this.fileSystem.ProcessFile, assemblyName))
                {
                    await projectDataManager.AddDataFileAsync(automationProject, this.ProjectVersion, this.fileSystem.ProcessFile, automationProject.ProjectId);
                }

                //Replace the assembly name in all of the fixture and test process file and save these files if there was a replacement done
                foreach (var directory in Directory.GetDirectories(this.fileSystem.TestCaseRepository))
                {
                    var processFiles = Directory.GetFiles(directory, "*.proc", SearchOption.AllDirectories);
                    foreach(var processFile in processFiles)
                    {
                        if(UpdateAssemblyReference(processFile, assemblyName))
                        {
                            //Fixtures and Test process files are named based on Identifier. This is what we need to tag these files with.
                            await projectDataManager.AddDataFileAsync(automationProject, this.ProjectVersion, processFile, Path.GetFileNameWithoutExtension(processFile));
                        }
                    }                   
                }

                //save the updates to the project version
                await projectDataManager.UpdateProjectVersionAsync(this.automationProject, this.ProjectVersion);

                logger.Information($"Assembly references updated in process and test files.");

                NotifyOfPropertyChange(() => CanPublish);
            }            
        }

        /// <summary>
        /// At design time, assembly names are generated with name_{digit}.dll syntax. Replace this in process file with specified assembly name.
        /// </summary>
        /// <param name="processFile"></param>
        /// <param name="assemblyName"></param>
        private bool UpdateAssemblyReference(string processFile, string assemblyName)
        {
            string fileContents = File.ReadAllText(processFile);
            Regex regex = new Regex($"({assemblyName})(_\\d+)");
            bool isMatch = regex.Matches(fileContents).Any();
            fileContents = regex.Replace(fileContents, (m) =>
            {
                return assemblyName;
            });
            File.WriteAllText(processFile, fileContents);
            return isMatch;
        }

    }
}
