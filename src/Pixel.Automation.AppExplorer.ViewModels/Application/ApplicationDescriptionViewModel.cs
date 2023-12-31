using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System.Windows.Documents;

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
        public ICollection<ApplicationScreen> Screens
        {
            get => applicationDescription.Screens;
        }

        /// <summary>
        /// Tracks available screens and active screen
        /// </summary>
        public ApplicationScreenCollection ScreenCollection { get; private set; }
        
        /// <summary>
        /// Controls belonging to the application
        /// </summary>
        public List<ControlDescriptionViewModel> ControlsCollection { get; set; } = new ();

        /// <summary>
        /// Prefabs belonging to the application
        /// </summary>
        public List<PrefabProjectViewModel> PrefabsCollection { get; set; } = new ();


        public ApplicationScreen this[string screenName]
        {
            get => Screens.Single(s => s.ScreenName.Equals(screenName));           
        }
            

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
        /// Check if a screen with given name is available
        /// </summary>
        /// <param name="screenName"></param>
        /// <returns></returns>
        public bool ContainsScreen(string screenName)
        {
            return this.Screens.Any(s => s.ScreenName.Equals(screenName));
        }

        /// <summary>
        /// Add a new screen to the application
        /// </summary>
        /// <param name="screenName"></param>
        public void AddScreen(ApplicationScreen screenName)
        {
            if(!this.Screens.Any(a => a.ScreenName.Equals(screenName)))
            {               
                this.Screens.Add(screenName);
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
            if(this.Screens.Any(a => a.ScreenName.Equals(currentName)))
            {    
                var screen = this.Screens.Single(s => s.ScreenName.Equals(currentName));
                screen.ScreenName = newName;
                this.ScreenCollection.RefreshScreens();
            }         
        }

        /// <summary>
        /// Remove an existing screen from the application.
        /// </summary>
        /// <param name="screenName"></param>
        public void DeleteScreen(string screenName)
        {
            if (this.Screens.Any(a => a.ScreenName.Equals(screenName)))
            {
                this.Screens.Remove(this.Screens.Single(s => s.ScreenName.Equals(screenName)));               
                this.ScreenCollection.RefreshScreens();
            }
        }

        /// <summary>
        /// Add a new control to the application.
        /// </summary>
        /// <param name="controlDescription"></param>
        public void AddControl(ControlDescriptionViewModel controlDescription, string screenName)
        {
            var screen = this.Screens.Single(s => s.ScreenName.Equals(screenName));
            var controlCollection = screen.AvailableControls;
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
            var screen = this.Screens.Single(s => s.ScreenName.Equals(screenName));
            var controlCollection = screen.AvailableControls;
            if (controlCollection.Contains(controlDescription.ControlId))
            {
                controlCollection.Remove(controlDescription.ControlId);
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
            DeleteControl(controlDescription, currentScreen);
            AddControl(controlDescription, moveToScreen);
        }

        /// <summary>
        /// Add a new prefab to the application
        /// </summary>
        /// <param name="prefabProject"></param>
        public void AddPrefab(PrefabProjectViewModel prefabProject, string screenName)
        {
            var screen = this.Screens.Single(s => s.ScreenName.Equals(screenName));
            var prefabCollection = screen.AvailablePrefabs;
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
            var screen = this.Screens.Single(s => s.ScreenName.Equals(screenName));
            var prefabCollection = screen.AvailablePrefabs;
            if(prefabCollection.Contains(prefabProject.PrefabId))
            {
                prefabCollection.Remove(prefabProject.PrefabId);
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
            DeletePrefab(prefabProject, currentScreen);
            AddPrefab(prefabProject, moveToScreen);
        }
    }
}
