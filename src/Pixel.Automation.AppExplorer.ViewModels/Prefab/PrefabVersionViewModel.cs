using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System.IO;
using System.Text.RegularExpressions;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    public class PrefabVersionViewModel : PropertyChangedBase
    {
        private readonly ILogger logger = Log.ForContext<PrefabVersionViewModel>();

        private readonly PrefabProject prefabProject;
        private readonly PrefabVersion prefabVersion;
        private readonly IPrefabFileSystem fileSystem;
        private readonly Lazy<IReferenceManager> referenceManager;

        public Version Version
        {
            get => prefabVersion.Version;
            set => prefabVersion.Version = value;
        }

        public bool IsPublished
        {
            get => prefabVersion.IsPublished;            
        }

        /// <summary>
        /// Indicates if this version is the active version. Active version open for edit by default.
        /// </summary>
        public bool IsActive
        {
            get => prefabVersion.IsActive;
            set
            {
                //can be only set to false when version is active
                if (prefabVersion.IsActive && !value)
                {
                    prefabVersion.IsActive = value;
                }
                NotifyOfPropertyChange(() => IsActive);
                NotifyOfPropertyChange(() => IsPublished);
            }
        }

        public bool CanPublish { get; private set; }


        public string PrefabAssembly
        {
            get => prefabVersion.DataModelAssembly;
            private set
            {
                if (prefabVersion.IsPublished && !string.IsNullOrEmpty(value))
                {
                    prefabVersion.DataModelAssembly = value;
                }
                NotifyOfPropertyChange(() => PrefabAssembly);
            }
        }


        public PrefabVersionViewModel(PrefabProject prefabProject, PrefabVersion prefabVersion, IPrefabFileSystem fileSystem, IReferenceManagerFactory referenceManagerFactory)
        {         
            this.prefabProject = Guard.Argument(prefabProject, nameof(prefabProject)).NotNull();
            this.prefabVersion = Guard.Argument(prefabVersion, nameof(prefabVersion)).NotNull();
            this.fileSystem = Guard.Argument(fileSystem).NotNull().Value;
            this.referenceManager = new Lazy<IReferenceManager>(() => { return referenceManagerFactory.CreateReferenceManager( this.prefabProject.PrefabId, this.prefabVersion.ToString(), this.fileSystem); });
            this.CanPublish = !prefabVersion.IsPublished && !prefabVersion.Version.Equals(prefabProject.LatestActiveVersion.Version);
        }

        public async  Task<PrefabVersion> CloneAsync(IPrefabDataManager prefabDataManager)
        {
            PrefabVersion newVersion;
            if (!this.prefabProject.AvailableVersions.Any(a => a.Version.Major.Equals(this.prefabVersion.Version.Major + 1)))
            {
                newVersion = new PrefabVersion(new Version(this.prefabVersion.Version.Major + 1, 0, 0, 0))
                {
                    IsActive = true
                };
            }
            else
            {               
                var versionsWithSameMajor = this.prefabProject.AvailableVersions.Select(s => s.Version).Where(v => v.Major.Equals(this.prefabVersion.Version.Major));
                int nextMinor = versionsWithSameMajor.Select(v => v.Minor).Max() + 1;
                newVersion = new PrefabVersion(new Version(this.prefabVersion.Version.Major, nextMinor, 0, 0))
                {
                    IsActive = true
                };
            }

            //Download the prefab data given it might not be locally available and then add new version by cloning from current version
            await prefabDataManager.DownloadPrefabDataAsync(this.prefabProject, this.prefabVersion);            
            await prefabDataManager.AddPrefabVersionAsync(this.prefabProject, newVersion, this.prefabVersion);
            return newVersion;
        }


        /// <summary>
        /// Copy last compiled dll from temp folder to References folder.
        /// Set IsDeployed to true and set the assembly name
        /// </summary>
        public async Task PublishAsync(IWorkspaceManagerFactory workspaceFactory, IPrefabDataManager prefabDataManager)
        {           
            if(!this.IsPublished)
            {
                this.fileSystem.Initialize(this.prefabProject, this.prefabVersion);
                logger.Information($"Prefab file system has been initialized.");

                string prefabProjectName = this.prefabProject.PrefabName;
                ICodeWorkspaceManager workspaceManager = workspaceFactory.CreateCodeWorkspaceManager(this.fileSystem.DataModelDirectory);
                workspaceManager.WithAssemblyReferences(this.referenceManager.Value.GetCodeEditorReferences());
                workspaceManager.AddProject(prefabProjectName, this.prefabProject.Namespace, Array.Empty<string>());
                string[] existingDataModelFiles = Directory.GetFiles(this.fileSystem.DataModelDirectory, "*.cs");
                if (existingDataModelFiles.Any())
                {
                    //This will add all the documents to workspace so that they are available during compilation
                    foreach (var dataModelFile in existingDataModelFiles)
                    {
                        string documentName = Path.GetFileName(dataModelFile);
                        if (!workspaceManager.HasDocument(documentName, prefabProjectName))
                        {
                            workspaceManager.AddDocument(documentName, prefabProjectName, File.ReadAllText(dataModelFile));
                        }
                    }
                }

                string assemblyName = $"{this.prefabProject.Namespace}.v{Version.Major}";
                using (var compilationResult = workspaceManager.CompileProject(prefabProjectName, assemblyName))
                {
                    compilationResult.SaveAssemblyToDisk(this.fileSystem.ReferencesDirectory);
                }
                logger.Information($"Workspace prepared and compilation done.");

                this.IsActive = false;
                this.CanPublish = false;
                this.PrefabAssembly = $"{assemblyName}.dll";

                logger.Information($"Data model assembly name is : {this.PrefabAssembly}");
             
                //Replace the assemly name in the process and template file
                UpdateAssemblyReference(this.fileSystem.PrefabFile, this.prefabProject.Namespace, assemblyName);
             
                if (File.Exists(this.fileSystem.TemplateFile))
                {
                    UpdateAssemblyReference(this.fileSystem.TemplateFile, this.prefabProject.Namespace, assemblyName);                   
                }

                logger.Information($"Assembly references updated in process and template files.");

                //save all the files belonging to Prefab
                await prefabDataManager.UpdatePrefabVersionAsync(this.prefabProject, this.prefabVersion);
                await prefabDataManager.SavePrefabDataAsync(this.prefabProject, this.prefabVersion);

                NotifyOfPropertyChange(() => CanPublish);
            }            
        }

        /// <summary>
        /// At design time, assembly names are generated with name_{digit}.dll syntax. Replace this in process file with specified assembly name.
        /// </summary>
        /// <param name="processFile"></param>
        /// <param name="assemblyName"></param>
        private void UpdateAssemblyReference(string processFile, string assemblyName, string replaceWith)
        {
            string fileContents = File.ReadAllText(processFile);
            Regex regex = new Regex($"({assemblyName})(_\\d+)");
            fileContents = regex.Replace(fileContents, (m) =>
            {
                return replaceWith;
            });
            File.WriteAllText(processFile, fileContents);

        }

    }
}
