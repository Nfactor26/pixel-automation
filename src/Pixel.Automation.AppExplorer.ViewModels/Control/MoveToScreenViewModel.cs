using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Editor.Core;

namespace Pixel.Automation.AppExplorer.ViewModels.Control
{
    /// <summary>
    /// Allow users to pick a new screen name for control or prefab
    /// </summary>
    public class MoveToScreenViewModel : SmartScreen 
    {    
        /// <summary>
        /// Available screen names
        /// </summary>
        public BindableCollection<string> Screens { get; private set; } = new ();

        private string selectedScreen;

        /// <summary>
        /// Selected screen name
        /// </summary>
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

        public string Name { get; private set; }

        public bool CanMoveToScreen
        {
            get
            {
                return !this.HasErrors && !string.IsNullOrEmpty(this.SelectedScreen);
            }
        }


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="screens"></param>
        public MoveToScreenViewModel(string name, IEnumerable<string> screens, string currentScreen)
        {
            this.DisplayName = "Move To Screen";         
            Guard.Argument(screens, nameof(screens)).NotNull();
            this.Screens.AddRange(screens.Except(new[] { currentScreen }));
            this.SelectedScreen = this.Screens.FirstOrDefault();
            this.Name = Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
        }

       
        private void ValidateScreenName(string screenName)
        {
            ClearErrors(nameof(SelectedScreen));
            ValidateRequiredProperty(nameof(SelectedScreen), screenName);           
        }

        /// <summary>
        /// Close the dialog with true result
        /// </summary>
        /// <returns></returns>
        public async Task MoveToScreen()
        {
            if (this.CanMoveToScreen)
            {
                await this.TryCloseAsync(true);
            }
        }

        /// <summary>
        /// Close the dialog with false result
        /// </summary>
        public async void Cancel()
        {
            await this.TryCloseAsync(false);
        }
    }
}
