using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System.Collections.Generic;

namespace Pixel.Automation.AppExplorer.ViewModels.Application
{
    /// <summary>
    /// View model for <see cref="ApplicationDescription"/>
    /// </summary>
    public class ApplicationDescriptionViewModel : NotifyPropertyChanged
    {
        private readonly ApplicationDescription applicationDescription;

        /// <summary>
        /// Wrapped instance <see cref="ApplicationDescription"/>
        /// </summary>
        public ApplicationDescription Model
        {
            get => applicationDescription;
        }
       
        /// <summary>
        /// Unique identifier for the application
        /// </summary>
        public string ApplicationId
        {
            get => applicationDescription.ApplicationId;
        }

        /// <summary>
        /// Name of the application
        /// </summary>
        public string ApplicationName
        {
            get => applicationDescription.ApplicationName;
            set
            {
                applicationDescription.ApplicationName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Type of application e.g. web or windows etc.
        /// </summary>
        public string ApplicationType
        {
            get => applicationDescription.ApplicationType;
            set
            {
                applicationDescription.ApplicationType = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Details of the application. This will be based on the type e.g. web application will have preferred browser and url
        /// while a windows application will have a executable and working directory etc.
        /// </summary>
        public IApplication ApplicationDetails
        {
            get => applicationDescription.ApplicationDetails;
        }

        /// <summary>
        /// Identifier of the controls belonging to this application
        /// </summary>
        public List<string> AvailableControls
        {
            get => applicationDescription.AvailableControls;
        }

        /// <summary>
        /// Identifier of the prefabs belonging to this application
        /// </summary>
        public List<string> AvailablePrefabs
        {
            get => applicationDescription.AvailablePrefabs;
        }
 
        /// <summary>
        /// Controls belonging to the application
        /// </summary>
        public List<ControlDescriptionViewModel> ControlsCollection { get; set; } = new List<ControlDescriptionViewModel>();

        /// <summary>
        /// Prefabs belonging to the application
        /// </summary>
        public List<PrefabProject> PrefabsCollection { get; set; } = new List<PrefabProject>();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="applicationDescription">Wrapped instance of <see cref="ApplicationDescription"/></param>
        public ApplicationDescriptionViewModel(ApplicationDescription applicationDescription)
        {
            this.applicationDescription = Guard.Argument(applicationDescription).NotNull();
        }       

        /// <summary>
        /// Add a new control to the application.
        /// </summary>
        /// <param name="controlDescription"></param>
        public void AddControl(ControlDescriptionViewModel controlDescription)
        {
            if (!this.AvailableControls.Contains(controlDescription.ControlId))
            {
                this.AvailableControls.Add(controlDescription.ControlId);
            }
            if (!this.ControlsCollection.Contains(controlDescription))
            {
                this.ControlsCollection.Add(controlDescription);
            }
        }

        /// <summary>
        /// Remove an existing control from the application
        /// </summary>
        /// <param name="controlDescription"></param>
        public void DeleteControl(ControlDescriptionViewModel controlDescription)
        {
            if (!this.AvailableControls.Contains(controlDescription.ControlId))
            {
                this.AvailableControls.Remove(controlDescription.ControlId);
            }
            if (this.ControlsCollection.Contains(controlDescription))
            {
                this.ControlsCollection.Remove(controlDescription);
            }
        }

        /// <summary>
        /// Add a new prefab to the application
        /// </summary>
        /// <param name="prefabProject"></param>
        public void AddPrefab(PrefabProject prefabProject)
        {
            if (!this.AvailablePrefabs.Contains(prefabProject.PrefabId))
            {
                this.AvailablePrefabs.Add(prefabProject.PrefabId);
            }
            if (!this.PrefabsCollection.Contains(prefabProject))
            {
                this.PrefabsCollection.Add(prefabProject);
            }
        }

        /// <summary>
        /// Remove an existing prefab from the application
        /// </summary>
        /// <param name="prefabProject"></param>
        public void DeletePrefab(PrefabProject prefabProject)
        {
            if (this.AvailablePrefabs.Contains(prefabProject.PrefabId))
            {
                this.AvailablePrefabs.Remove(prefabProject.PrefabId);
            }
            if (this.PrefabsCollection.Contains(prefabProject))
            {
                this.PrefabsCollection.Remove(prefabProject);
            }
        }
    }
}
