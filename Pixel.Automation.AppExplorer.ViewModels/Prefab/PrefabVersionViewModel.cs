using Pixel.Automation.Core.Models;
using System;
using System.IO;
using System.Linq;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    public class PrefabVersionViewModel
    {
        private readonly PrefabDescription prefabDescription;
        private readonly PrefabVersion prefabVersion;

        public Version Version
        {
            get => prefabVersion.Version;
            set => prefabVersion.Version = value;
        }

        public bool IsDeployed
        {
            get => prefabVersion.IsDeployed;
            private set
            {
                if (!prefabVersion.IsDeployed && value)
                {
                    prefabVersion.IsDeployed = value;
                }
            }
        }

        public string PrefabAssembly
        {
            get => prefabVersion.PrefabAssembly;
            private set
            {
                if (!prefabVersion.IsDeployed && !string.IsNullOrEmpty(value))
                {
                    prefabVersion.PrefabAssembly = value;
                }
            }
        }

        /// <summary>
        /// Indicates if this version is the active version. Active version open for edit by default.
        /// </summary>
        public bool IsActive
        {
            get => prefabVersion.IsActive;
            private set
            {
                if (!prefabVersion.IsDeployed)
                {
                    prefabVersion.IsActive = value;
                }
            }
        }

        public PrefabVersionViewModel(PrefabDescription prefabDescription, PrefabVersion prefabVersion)
        {
            this.prefabDescription = prefabDescription;
            this.prefabVersion = prefabVersion;
        }

        /// <summary>
        /// Copy last compiled dll from temp folder to References folder.
        /// Set IsDeployed to true and set the assembly name
        /// </summary>
        public void Deploy()
        {
            string prefabDirectory = Path.Combine("ApplicationsRepository", this.prefabDescription.ApplicationId, "Prefabs", this.prefabDescription.PrefabId, this.prefabVersion.Version.ToString());
            string tempDirectory = Path.Combine(prefabDirectory, "Temp");
            var assemblyFiles = Directory.GetFiles(tempDirectory, "*.dll").Select(f => new FileInfo(f));
            var targetFile = assemblyFiles.OrderBy(a => a.CreationTime).Last();

            string deployedAssemblyName = $"{this.prefabDescription.PrefabName}.dll";
            string referencesDirectory = Path.Combine(prefabDirectory, "References");
            File.Copy(targetFile.FullName, Path.Combine(referencesDirectory, deployedAssemblyName));
       
            prefabVersion.IsDeployed = true;
            prefabVersion.PrefabAssembly = Path.Combine(referencesDirectory, deployedAssemblyName);
        }

        /// <summary>
        /// Set the IsActive Property of version to false.
        /// </summary>
        public void Delete()
        {
            if(prefabVersion.IsDeployed)
            {
                prefabVersion.IsActive = false;
            }
        }
    
    }
}
