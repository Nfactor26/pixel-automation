using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    public class PrefabProjectViewModel : NotifyPropertyChanged
    {
        private readonly PrefabProject prefabProject;
        public PrefabProject PrefabProject 
        {
            get => this.prefabProject;
        }
       
        [Browsable(false)]
        public string ApplicationId
        {
            get => this.prefabProject.ApplicationId;
        }

    
        [Browsable(false)]
        public string PrefabId
        {
            get => this.prefabProject.PrefabId;            
        }

        /// <summary>
        /// Display name that should be visible in Prefab Repository
        /// </summary>      
        public string PrefabName
        {
            get => this.prefabProject.PrefabName;
            set
            {
                this.prefabProject.PrefabName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// NameSpace for generated models. NameSpace must be unique
        /// </summary>     
        public string NameSpace
        {
            get => this.prefabProject.NameSpace;
            set
            {
                this.prefabProject.NameSpace = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Group name used to group prefab on UI
        /// </summary>
        public string GroupName
        {
            get => this.prefabProject.GroupName;         
            set
            {
                this.prefabProject.GroupName = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<PrefabVersion> DeployedVersions 
        { 
            get => this.prefabProject.AvailableVersions.Where(a => a.IsDeployed).ToList(); 
        }

        public PrefabProjectViewModel(PrefabProject prefabProject)
        {
            this.prefabProject = Guard.Argument(prefabProject).NotNull();
        }
    }
}
