using Dawn;
using Pixel.Automation.Editor.Core;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.AppExplorer.ViewModels.Application
{
    public class RenameScreenViewModel : SmartScreen
    {
        private readonly ApplicationDescriptionViewModel applicationDescriptionViewModel;

      
        public string ScreenName { get; private set; }
        

        private string newScreenName;

        public string NewScreenName
        {
            get => newScreenName;
            set
            {
                newScreenName = value;
                ValidateScreenName(value);
                NotifyOfPropertyChange(() => NewScreenName);
                NotifyOfPropertyChange(() => CanRenameScreen);
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="applicationDescriptionViewModel"></param>
        /// <param name="screenName"></param>
        public RenameScreenViewModel(ApplicationDescriptionViewModel applicationDescriptionViewModel, string screenName)
        {
            this.DisplayName = "Rename Screen";
            this.applicationDescriptionViewModel = Guard.Argument(applicationDescriptionViewModel).NotNull();
            this.ScreenName = Guard.Argument(screenName).NotNull().NotEmpty();
        }

        /// <summary>
        /// Rename screen to a new value
        /// </summary>
        /// <returns></returns>
        public async Task RenameScreen()
        {
            if (this.CanRenameScreen)
            {
                this.applicationDescriptionViewModel.RenameScreen(this.ScreenName, this.NewScreenName);
                await this.TryCloseAsync(true);
            }
        }

        /// <summary>
        /// Check if it is possible to rename screen
        /// </summary>
        public bool CanRenameScreen
        {
            get
            {
                return !this.HasErrors && !string.IsNullOrEmpty(this.NewScreenName) && IsNameAvailable(this.NewScreenName);
            }
        }

        /// <summary>
        /// Validate screen name
        /// </summary>
        /// <param name="newScreenName"></param>
        private void ValidateScreenName(string newScreenName)
        {
            ClearErrors(nameof(NewScreenName));
            ValidateRequiredProperty(nameof(NewScreenName), newScreenName);
            ValidatePattern("[A-Za-z]{3,}", nameof(NewScreenName), newScreenName, "Name must contain only alphabets and should be atleast 3 characters in length.");
            if (!IsNameAvailable(newScreenName))
            {
                this.AddOrAppendErrors(nameof(NewScreenName), $"Screen already exists with name {NewScreenName}");
            }
        }

        /// <summary>
        /// Check if new screen name is available
        /// </summary>
        /// <param name="screenName"></param>
        /// <returns></returns>
        private bool IsNameAvailable(string screenName)
        {
            return !this.applicationDescriptionViewModel.ScreensCollection.Any(a => a.Equals(screenName));
        }

        /// <summary>
        /// Cancel rename screen operation
        /// </summary>
        public async void Cancel()
        {
            await this.TryCloseAsync(false);
        }
    }
}
