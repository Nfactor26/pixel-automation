using Caliburn.Micro;
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
        private readonly PrefabDescription prefabDescription;
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
                if (!prefabVersion.IsDeployed && value)
                {
                    prefabVersion.IsDeployed = value;
                }
                NotifyOfPropertyChange(() => IsDeployed);
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

        /// <summary>
        /// Indicates if this version is the active version. Active version open for edit by default.
        /// </summary>
        public bool IsActive
        {
            get => prefabVersion.IsActive;
            set
            {
                if (!prefabVersion.IsDeployed)
                {
                    prefabVersion.IsActive = value;
                }
                NotifyOfPropertyChange(() => IsActive);
            }
        }

        public PrefabVersionViewModel(PrefabDescription prefabDescription, PrefabVersion prefabVersion, IPrefabFileSystem fileSystem)
        {
            this.prefabDescription = prefabDescription;
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

            this.fileSystem.Initialize(this.prefabDescription.ApplicationId, this.prefabDescription.PrefabId, this.prefabVersion);
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
            //string prefabDirectory = Path.Combine("ApplicationsRepository", this.prefabDescription.ApplicationId, "Prefabs", this.prefabDescription.PrefabId, this.prefabVersion.Version.ToString());
            //string tempDirectory = Path.Combine(prefabDirectory, "Temp");
            //var assemblyFiles = Directory.GetFiles(tempDirectory, "*.dll").Select(f => new FileInfo(f));
            //var targetFile = assemblyFiles.OrderBy(a => a.CreationTime).Last();

            //string deployedAssemblyName = $"{this.prefabDescription.PrefabName}.dll";
            //string referencesDirectory = Path.Combine(prefabDirectory, "References");
            //File.Copy(targetFile.FullName, Path.Combine(referencesDirectory, deployedAssemblyName));

            //prefabVersion.IsDeployed = true;
            //prefabVersion.PrefabAssembly = Path.Combine(referencesDirectory, deployedAssemblyName);
           
            this.fileSystem.Initialize(this.prefabDescription.ApplicationId, this.prefabDescription.PrefabId, this.prefabVersion);

            ICodeWorkspaceManager workspaceManager = workspaceFactory.CreateCodeWorkspaceManager(this.fileSystem.DataModelDirectory);
            workspaceManager.WithAssemblyReferences(this.fileSystem.GetAssemblyReferences());
            workspaceManager.AddProject(this.prefabDescription.GetPrefabName(), $"Pixel.Automation.{this.prefabDescription.GetPrefabName()}", Array.Empty<string>());
            string[] existingDataModelFiles = Directory.GetFiles(this.fileSystem.DataModelDirectory, "*.cs");
            if (existingDataModelFiles.Any())
            {
                //This will add all the documents to workspace so that they are available during compilation
                foreach (var dataModelFile in existingDataModelFiles)
                {
                    string documentName = Path.GetFileName(dataModelFile);
                    if (!workspaceManager.HasDocument(documentName, this.prefabDescription.PrefabName))
                    {
                        workspaceManager.AddDocument(documentName, this.prefabDescription.PrefabName, File.ReadAllText(dataModelFile));
                    }
                }
            }

            string assemblyName = this.prefabDescription.GetPrefabName();
            using (var compilationResult = workspaceManager.CompileProject(this.prefabDescription.PrefabName, assemblyName))
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
