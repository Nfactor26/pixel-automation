using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    public class PrefabVersionViewModel : PropertyChangedBase
    {
        private readonly PrefabProject prefabProject;
        private readonly PrefabVersion prefabVersion;
        private readonly IPrefabFileSystem fileSystem;

        public Version Version
        {
            get => prefabVersion.Version;
            set => prefabVersion.Version = value;
        }

        public bool IsDeployed
        {
            get => prefabVersion.IsDeployed;
            set
            {
                //can be only set to true when version is not already deployed
                if (!prefabVersion.IsDeployed && value)
                {
                    prefabVersion.IsDeployed = value;
                }
                NotifyOfPropertyChange(() => IsDeployed);
            }
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
            }
        }


        public string PrefabAssembly
        {
            get => prefabVersion.DataModelAssembly;
            private set
            {
                if (prefabVersion.IsDeployed && !string.IsNullOrEmpty(value))
                {
                    prefabVersion.DataModelAssembly = value;
                }
                NotifyOfPropertyChange(() => PrefabAssembly);
            }
        }


        public PrefabVersionViewModel(PrefabProject prefabProject, PrefabVersion prefabVersion, IPrefabFileSystem fileSystem)
        {
            this.prefabProject = prefabProject;
            this.prefabVersion = prefabVersion;
            this.fileSystem = fileSystem;
        }

        public PrefabVersion Clone()
        {
            ////Increment active version for project        
            PrefabVersion newVersionInfo = new PrefabVersion(new Version(this.prefabVersion.Version.Major + 1, 0, 0, 0))
            {
                IsActive = true,
                IsDeployed = false
            };

            this.fileSystem.Initialize(this.prefabProject.ApplicationId, this.prefabProject.PrefabId, this.prefabVersion);
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

            return newVersionInfo;
        }


        /// <summary>
        /// Copy last compiled dll from temp folder to References folder.
        /// Set IsDeployed to true and set the assembly name
        /// </summary>
        public void Deploy(IWorkspaceManagerFactory workspaceFactory)
        {           
           
            this.fileSystem.Initialize(this.prefabProject.ApplicationId, this.prefabProject.PrefabId, this.prefabVersion);

            string prefabProjectName = this.prefabProject.GetPrefabName();
            ICodeWorkspaceManager workspaceManager = workspaceFactory.CreateCodeWorkspaceManager(this.fileSystem.DataModelDirectory);
            workspaceManager.WithAssemblyReferences(this.fileSystem.GetAssemblyReferences());
            workspaceManager.AddProject(prefabProjectName, $"{Constants.PrefabDataModelName}.{this.prefabProject.GetPrefabName()}", Array.Empty<string>());
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

            string assemblyName = this.prefabProject.GetPrefabName();
            using (var compilationResult = workspaceManager.CompileProject(prefabProjectName, assemblyName))
            {
                compilationResult.SaveAssemblyToDisk(this.fileSystem.ReferencesDirectory);
            }
            this.IsDeployed = true;
            this.IsActive = false;
            this.PrefabAssembly = $"{assemblyName}.dll";

            //Replace the assemly name in the process and template file
            UpdateAssemblyReference(this.fileSystem.PrefabFile, assemblyName);
            if(File.Exists(this.fileSystem.TemplateFile))
            {
                UpdateAssemblyReference(this.fileSystem.TemplateFile, assemblyName);
            }

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
