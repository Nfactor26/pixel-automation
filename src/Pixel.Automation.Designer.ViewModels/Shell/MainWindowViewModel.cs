using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels.Shell
{
    public class MainWindowViewModel : Conductor<IShell>.Collection.OneActive
    {
        private readonly ILogger logger = Log.ForContext<MainWindowViewModel>();
        private readonly ISignInManager signInManager;
        private readonly IApplicationDataManager applicationDataManager;

        public BindableCollection<IFlyOut> FlyOuts { get; } = new BindableCollection<IFlyOut>();


        public MainWindowViewModel(ApplicationSettings applicationSettings, ISignInManager signinManager,
            IApplicationDataManager applicationDataManager,
            IEnumerable<IShell> shells, IEnumerable<IFlyOut> flyOuts)
        {
            Guard.Argument(shells, nameof(shells)).NotNull().NotEmpty();
            Guard.Argument(flyOuts, nameof(flyOuts)).NotNull().NotEmpty();
            this.signInManager = Guard.Argument(signinManager).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager).NotNull().Value;

            this.Items.AddRange(shells);
            this.FlyOuts.AddRange(flyOuts);

            this.DisplayName = "Pixel Automation";
            if (applicationSettings.IsOfflineMode)
            {
                logger.Information("Application is running in offline mode.");
                _ = this.ActivateItemAsync(this.Items.OfType<DesignerWindowViewModel>().FirstOrDefault());
            }
            else
            {
                logger.Information("Application is running in online mode.");
                this.signInManager.SignInCompletedAsync += OnSignInCompletedAsync;
                _ = this.ActivateItemAsync(this.Items.OfType<LoginWindowViewModel>().FirstOrDefault());
            }
        }
        public void ToggleFlyout(int index)
        {
            var flyout = this.FlyOuts[index];
            flyout.IsOpen = !flyout.IsOpen;
        }


        private async Task OnSignInCompletedAsync(object? sender, SignInCompletedEventArgs e)
        {
            logger.Information("Processing sign in completed event");
            this.signInManager.SignInCompletedAsync -= OnSignInCompletedAsync;
            await this.DeactivateItemAsync(this.Items.OfType<LoginWindowViewModel>().FirstOrDefault(), true);
            if (e.IsSuccess && this.signInManager.IsUserAuthorized())
            {
                System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Maximized;
                await this.applicationDataManager.DownloadApplicationsDataAsync();
                logger.Information("Download of application data completed");
                await this.applicationDataManager.DownloadProjectsAsync();
                logger.Information("Download of project information completed");

                await this.ActivateItemAsync(this.Items.OfType<DesignerWindowViewModel>().FirstOrDefault());

                return;
            }
            logger.Information("User doesn't have required authorization to use application");
        }
    }
}
