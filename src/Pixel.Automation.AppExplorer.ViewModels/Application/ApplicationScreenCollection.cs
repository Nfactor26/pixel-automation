using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;

namespace Pixel.Automation.AppExplorer.ViewModels.Application
{
    /// <summary>
    /// ApplicationScreenCollection holds all the screens belonging to an application and tracks the SelectedScreen
    /// </summary>
    public class ApplicationScreenCollection : NotifyPropertyChanged
    {
        private readonly ApplicationDescriptionViewModel applicationDescription;

        /// <summary>
        /// Screens belonging to application
        /// </summary>
        public BindableCollection<string> Screens { get; private set; } = new();

        private string selectedScreen;

        /// <summary>
        /// Currently selected screen
        /// </summary>
        public string SelectedScreen
        {
            get => selectedScreen;
            set
            {
                selectedScreen = value;
                OnScreenChanged();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="applicationDescription"></param>
        public ApplicationScreenCollection(ApplicationDescriptionViewModel applicationDescription)
        {
            this.applicationDescription = Guard.Argument(applicationDescription, nameof(applicationDescription)).NotNull();
            this.Screens.Clear();
            this.Screens.AddRange(applicationDescription.Screens);
            if(this.Screens.Any())
            {
                this.SelectedScreen = this.Screens.First();
            }
        }

        /// <summary>
        /// Change the active screen
        /// </summary>
        /// <param name="screenName"></param>
        public void SetActiveScreen(string screenName)
        {
            if (this.Screens.Contains(screenName))
            {
                this.SelectedScreen = screenName;
            }
        }

        /// <summary>
        /// Refresh screens from owner application.
        /// </summary>
        public void RefreshScreens()
        {
            this.Screens.Clear();
            this.Screens.AddRange(applicationDescription.Screens);
        }

        public event EventHandler<string> ScreenChanged = delegate {};

        protected virtual void OnScreenChanged()
        {
            this.ScreenChanged(this, this.SelectedScreen);
        }
    }
}
