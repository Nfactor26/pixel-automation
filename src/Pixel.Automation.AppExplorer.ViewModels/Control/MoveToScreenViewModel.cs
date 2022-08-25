using Caliburn.Micro;
using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.Editor.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.AppExplorer.ViewModels.Control
{
    public class MoveToScreenViewModel : SmartScreen
    {
        private readonly ApplicationDescriptionViewModel applicationDescription;
        private readonly ControlDescriptionViewModel controlDescription;
        private readonly string currentScreen;
      
        public BindableCollection<string> Screens { get; private set; } = new ();

        private string selectedScreen;

        public string SelectedScreen
        {
            get => this.selectedScreen;
            set
            {
                this.selectedScreen = value;
                ValidateScreenName(selectedScreen);
                NotifyOfPropertyChange(nameof(SelectedScreen));
                NotifyOfPropertyChange(nameof(CanMoveToScreen));
            }
        }

        public string ControlName { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <param name="controlDescription"></param>
        /// <param name="screens"></param>
        public MoveToScreenViewModel(ApplicationDescriptionViewModel applicationDescription, ControlDescriptionViewModel controlDescription,
            string currentScreen, IEnumerable<string> screens)
        {
            this.DisplayName = "Move Control To Screen";
            this.applicationDescription = Guard.Argument(applicationDescription).NotNull();
            this.controlDescription = Guard.Argument(controlDescription).NotNull();
            this.currentScreen = Guard.Argument(currentScreen).NotNull().NotEmpty();
            Guard.Argument(screens).NotNull();
            this.Screens.AddRange(screens.Except(new[] { currentScreen }));
            this.SelectedScreen = this.Screens.FirstOrDefault();
            this.ControlName = controlDescription.ControlName;
        }

        public async Task MoveToScreen()
        {
            if (this.CanMoveToScreen)
            {          
                this.applicationDescription.MoveControlToScreen(controlDescription, currentScreen, SelectedScreen);
                await this.TryCloseAsync(true);
            }
        }

        public bool CanMoveToScreen
        {
            get
            {
                return !this.HasErrors && !string.IsNullOrEmpty(this.SelectedScreen);
            }
        }

        private void ValidateScreenName(string screenName)
        {
            ClearErrors(nameof(SelectedScreen));
            ValidateRequiredProperty(nameof(SelectedScreen), screenName);           
        }

        public async void Cancel()
        {
            await this.TryCloseAsync(false);
        }
    }
}
