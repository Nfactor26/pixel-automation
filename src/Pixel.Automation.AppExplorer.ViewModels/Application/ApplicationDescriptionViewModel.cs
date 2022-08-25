using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System.Collections.Generic;
using System.Linq;

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
        /// Collection of screens for application used to group controls
        /// </summary>
        public IEnumerable<string> ScreensCollection
        {
            get => AvailableControls.Keys.Select(s => s);
        }

        /// <summary>
        /// Control identifier collection for a given application screen
        /// </summary>
        public Dictionary<string, List<string>> AvailableControls
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
        public List<ControlDescriptionViewModel> ControlsCollection { get; set; } = new ();

        /// <summary>
        /// Prefabs belonging to the application
        /// </summary>
        public List<PrefabProjectViewModel> PrefabsCollection { get; set; } = new ();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="applicationDescription">Wrapped instance of <see cref="ApplicationDescription"/></param>
        public ApplicationDescriptionViewModel(ApplicationDescription applicationDescription)
        {
            this.applicationDescription = Guard.Argument(applicationDescription).NotNull();
        }       

        /// <summary>
        /// Add a new screen to the application
        /// </summary>
        /// <param name="screenName"></param>
        public void AddScreen(string screenName)
        {
            if(!this.ScreensCollection.Contains(screenName))
            {
                this.applicationDescription.AvailableControls.Add(screenName, new List<string>());
                OnPropertyChanged(nameof(ScreensCollection));
            }
        }

        /// <summary>
        /// Rename an existing screen
        /// </summary>
        /// <param name="currentName"></param>
        /// <param name="newName"></param>
        public void RenameScreen(string currentName, string newName)
        {
            if (this.AvailableControls.ContainsKey(currentName))
            {
                var controls = this.AvailableControls[currentName];
                this.AvailableControls.Remove(currentName);
                this.AddScreen(newName);             
                this.AvailableControls[newName].AddRange(controls);
            }
        }

        /// <summary>
        /// Remove an existing screen from the application.
        /// </summary>
        /// <param name="screenName"></param>
        public void DeleteScreen(string screenName)
        {
            if (this.ScreensCollection.Contains(screenName))
            {
                this.applicationDescription.AvailableControls.Remove(screenName);
                OnPropertyChanged(nameof(ScreensCollection));
            }
        }

        /// <summary>
        /// Add a new control to the application.
        /// </summary>
        /// <param name="controlDescription"></param>
        public void AddControl(ControlDescriptionViewModel controlDescription, string screenName)
        {
            AddScreen(screenName);
            var controlCollection = this.applicationDescription.AvailableControls[screenName];
            if (!controlCollection.Contains(controlDescription.ControlId))
            {
                controlCollection.Add(controlDescription.ControlId);
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
        public void DeleteControl(ControlDescriptionViewModel controlDescription, string screenName)
        {
            if(this.applicationDescription.AvailableControls.ContainsKey(screenName))
            {
                var controlCollection = this.applicationDescription.AvailableControls[screenName];
                if (controlCollection.Contains(controlDescription.ControlId))
                {
                    controlCollection.Remove(controlDescription.ControlId);
                }
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
        public void AddPrefab(PrefabProjectViewModel prefabProject)
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
        public void DeletePrefab(PrefabProjectViewModel prefabProject)
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
