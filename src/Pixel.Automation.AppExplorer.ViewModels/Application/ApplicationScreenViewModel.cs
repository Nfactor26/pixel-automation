using Dawn;
using Pixel.Automation.Editor.Core;

namespace Pixel.Automation.AppExplorer.ViewModels.Application
{
    public class ApplicationScreenViewModel : SmartScreen
    {
        private readonly ApplicationDescriptionViewModel applicationDescriptionViewModel;

        private string screenName;

        public string ScreenName
        {
            get => screenName;
            set
            {
                screenName = value;
                ValidateScreenName(value);
                NotifyOfPropertyChange(() => ScreenName);
                NotifyOfPropertyChange(() => CanCreateScreen);
            }
        }

        public ApplicationScreenViewModel(ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
            this.DisplayName = "Create New Application Screen";
            this.applicationDescriptionViewModel = Guard.Argument(applicationDescriptionViewModel).NotNull();
        }

        public async Task CreateNewScreen()
        {
            if(this.CanCreateScreen)
            {              
                await this.TryCloseAsync(true);               
            }           
        }

        public bool CanCreateScreen
        {
            get
            {
                return !this.HasErrors && !string.IsNullOrEmpty(this.ScreenName) && IsNameAvailable(this.ScreenName);
            }         
        }

        private void ValidateScreenName(string screenName)
        {
            ClearErrors(nameof(ScreenName));
            ValidateRequiredProperty(nameof(ScreenName), screenName);
            ValidatePattern("[A-Za-z]{3,}", nameof(ScreenName), screenName, "Name must contain only alphabets and should be atleast 3 characters in length.");
            if (!IsNameAvailable(screenName))
            {
                this.AddOrAppendErrors(nameof(ScreenName), $"Screen already exists with name {ScreenName}");
            }

        }

        private bool IsNameAvailable(string screenName)
        {
            return !this.applicationDescriptionViewModel.ScreenCollection.Screens.Any(a => a.Equals(screenName));
        }

        public async void Cancel()
        {
            await this.TryCloseAsync(false);
        }

    }
}
