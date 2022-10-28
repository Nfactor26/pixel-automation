using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System.ComponentModel;

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

        public IEnumerable<PrefabVersion> PublishedVersion 
        {
            get => this.prefabProject.PublishedVersions; 
        }

        private bool isOpenInEditor;
        /// <summary>
        /// Indicate if the project is open in editor
        /// </summary>
        public bool IsOpenInEditor
        {
            get => this.isOpenInEditor;
            set
            {
                this.isOpenInEditor = value;
                OnPropertyChanged();
            }
        }


        public PrefabProjectViewModel(PrefabProject prefabProject)
        {
            this.prefabProject = Guard.Argument(prefabProject).NotNull();
        }
    }
}
