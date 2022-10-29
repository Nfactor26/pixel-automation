using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

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
        public ICollection<string> Screens
        {
            get => applicationDescription.Screens;
        }

        /// <summary>
        /// Tracks available screens and active screen
        /// </summary>
        public ApplicationScreenCollection ScreenCollection { get; private set; }

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
        public Dictionary<string, List<string>> AvailablePrefabs
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
            this.ScreenCollection = new ApplicationScreenCollection(this);
        }       

        /// <summary>
        /// Add a new screen to the application
        /// </summary>
        /// <param name="screenName"></param>
        public void AddScreen(string screenName)
        {
            if(!this.Screens.Contains(screenName))
            {
                this.Screens.Add(screenName);
                this.applicationDescription.AvailableControls.Add(screenName, new List<string>());
                this.applicationDescription.AvailablePrefabs.Add(screenName, new List<string>());
                this.ScreenCollection.RefreshScreens();
            }
        }

        /// <summary>
        /// Rename an existing screen
        /// </summary>
        /// <param name="currentName"></param>
        /// <param name="newName"></param>
        public void RenameScreen(string currentName, string newName)
        {
            if(this.Screens.Contains(currentName))
            {
                var controls = this.AvailableControls[currentName];
                this.AvailableControls.Remove(currentName);
                var prefabs = this.AvailablePrefabs[currentName];
                this.AvailablePrefabs.Remove(currentName);
                this.Screens.Remove(currentName);
                this.AddScreen(newName);
                this.AvailableControls[newName].AddRange(controls);
                this.AvailablePrefabs[newName].AddRange(prefabs);
                this.ScreenCollection.RefreshScreens();
            }         
        }

        /// <summary>
        /// Remove an existing screen from the application.
        /// </summary>
        /// <param name="screenName"></param>
        public void DeleteScreen(string screenName)
        {
            if (this.Screens.Contains(screenName))
            {
                this.Screens.Remove(screenName);
                this.applicationDescription.AvailableControls.Remove(screenName);
                this.applicationDescription.AvailablePrefabs.Remove(screenName);
                this.ScreenCollection.RefreshScreens();
            }
        }

        /// <summary>
        /// Add a new control to the application.
        /// </summary>
        /// <param name="controlDescription"></param>
        public void AddControl(ControlDescriptionViewModel controlDescription, string screenName)
        {           
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
        /// Move Control from one screen to another
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <param name="currentScreen"></param>
        /// <param name="moveToScreen"></param>
        public void MoveControlToScreen(ControlDescriptionViewModel controlDescription, string currentScreen, string moveToScreen)
        {
            if (this.applicationDescription.AvailableControls.ContainsKey(currentScreen) && this.applicationDescription.AvailableControls.ContainsKey(moveToScreen))
            {
                var currentScreenControls = this.applicationDescription.AvailableControls[currentScreen];
                if (currentScreenControls.Contains(controlDescription.ControlId))
                {
                    currentScreenControls.Remove(controlDescription.ControlId);
                }

                var newScreenControls = this.applicationDescription.AvailableControls[moveToScreen];
                newScreenControls.Add(controlDescription.ControlId);
            }
        }

        /// <summary>
        /// Add a new prefab to the application
        /// </summary>
        /// <param name="prefabProject"></param>
        public void AddPrefab(PrefabProjectViewModel prefabProject, string screenName)
        {          
            var prefabCollection = this.applicationDescription.AvailablePrefabs[screenName];
            if (!prefabCollection.Contains(prefabProject.PrefabId))
            {
                prefabCollection.Add(prefabProject.PrefabId);
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
        public void DeletePrefab(PrefabProjectViewModel prefabProject, string screenName)
        {
            if (this.applicationDescription.AvailablePrefabs.ContainsKey(screenName))
            {
                var prefabCollection = this.applicationDescription.AvailablePrefabs[screenName];
                if (prefabCollection.Contains(prefabProject.PrefabId))
                {
                    prefabCollection.Remove(prefabProject.PrefabId);
                }
            }

            if (this.PrefabsCollection.Contains(prefabProject))
            {
                this.PrefabsCollection.Remove(prefabProject);
            }
        }

        /// <summary>
        /// Move Control from one screen to another
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <param name="currentScreen"></param>
        /// <param name="moveToScreen"></param>
        public void MovePrefabToScreen(PrefabProjectViewModel prefabProject, string currentScreen, string moveToScreen)
        {
            if (this.applicationDescription.AvailablePrefabs.ContainsKey(currentScreen) && this.applicationDescription.AvailablePrefabs.ContainsKey(moveToScreen))
            {
                var currentScreenPrefabs = this.applicationDescription.AvailablePrefabs[currentScreen];
                if (currentScreenPrefabs.Contains(prefabProject.PrefabId))
                {
                    currentScreenPrefabs.Remove(prefabProject.PrefabId);
                }

                var newScreenControls = this.applicationDescription.AvailablePrefabs[moveToScreen];
                newScreenControls.Add(prefabProject.PrefabId);
            }
        }
    }
}
