using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Persistence.Services.Client;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Pixel.Automation.AppExplorer.ViewModels.Application
{
    /// <summary>
    /// Application explorer view is used to manage the applications that are used in an automation project.
    /// </summary>
    public class ApplicationExplorerViewModel : AnchorableHost
    {
        #region data members

        private readonly ILogger logger = Log.ForContext<ApplicationExplorerViewModel>();

        private readonly IEventAggregator eventAggregator;
        private readonly ITypeProvider typeProvider;
        private readonly IApplicationDataManager applicationDataManager;
        private readonly IWindowManager windowManager;
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// Child views that are dependent on the selected application such as control explorer and prefab explorer
        /// </summary>
        public BindableCollection<IApplicationAware> ChildViews { get; private set; } = new BindableCollection<IApplicationAware>();

        private IApplicationAware selectedView;
        /// <summary>
        /// Selected child view
        /// </summary>
        public IApplicationAware SelectedView
        {
            get => selectedView;
            set
            {
                selectedView = value;
                NotifyOfPropertyChange(() => SelectedView);
            }
        }

        /// <summary>
        /// Collection of applications available 
        /// </summary>
        public BindableCollection<ApplicationDescriptionViewModel> Applications { get; set; } = new BindableCollection<ApplicationDescriptionViewModel>();

        /// <summary>
        /// Collection of known applications that can be added e.g. web application, windows application , etc
        /// </summary>
        public BindableCollection<KnownApplication> KnownApplications { get; set; } = new BindableCollection<KnownApplication>();

        private ApplicationDescriptionViewModel selectedApplication;
        /// <summary>
        /// Selected Application on the view
        /// </summary>
        public ApplicationDescriptionViewModel SelectedApplication
        {
            get => selectedApplication;
            set
            {
                selectedApplication = value;
                NotifyOfPropertyChange(() => SelectedApplication);
                CanEdit = false;
                if (value != null)
                {
                    //Notification for property grid to display selected application details
                    this.eventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(value.ApplicationDetails, true));
                    logger.Debug("Selected application set to {Application}", selectedApplication.ApplicationName);
                }

            }
        }

        #endregion data members

        #region constructor

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="applicationDataManager"></param>
        /// <param name="typeProvider"></param>
        /// <param name="childView"></param>
        public ApplicationExplorerViewModel(IEventAggregator eventAggregator, IApplicationDataManager applicationDataManager,
            ITypeProvider typeProvider, IEnumerable<IApplicationAware> childView, IWindowManager windowManager, INotificationManager notificationManager)
        {
            this.DisplayName = "Application Repository";
            this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value; ;
            this.typeProvider = Guard.Argument(typeProvider, nameof(typeProvider)).NotNull().Value; ;
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
            this.ChildViews.AddRange(childView);
            this.SelectedView = this.ChildViews[0];

            CreateCollectionView();
            InitializeKnownApplications();

            var applications = this.applicationDataManager.GetAllApplications();
            foreach (var application in applications)
            {
                if(!application.IsDeleted)
                {
                    this.Applications.Add(new ApplicationDescriptionViewModel(application));
                }
            }
            Applications.OrderBy(a => a.ApplicationName);
        }

        #endregion constructor

        #region Filter 


        string filterText = string.Empty;
        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;
                var view = CollectionViewSource.GetDefaultView(Applications);
                view.Refresh();
                NotifyOfPropertyChange(() => Applications);

            }
        }

        private void CreateCollectionView()
        {
            var groupedItems = CollectionViewSource.GetDefaultView(Applications);
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ApplicationDescriptionViewModel.ApplicationName)));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(ApplicationDescriptionViewModel.ApplicationName), ListSortDirection.Ascending));
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                return (a as ApplicationDescriptionViewModel).ApplicationName.ToLower().Contains(this.filterText.ToLower());
            });
        }

        #endregion Filter

        #region Manage Application

        public bool IsApplicationOpen { get; set; }

        public void OpenApplication(ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
            Guard.Argument(applicationDescriptionViewModel, nameof(applicationDescriptionViewModel)).NotNull();

            IsApplicationOpen = true;          
            foreach (var childView in ChildViews)
            {
                childView.SetActiveApplication(applicationDescriptionViewModel);
            }
            if (this.eventAggregator.HandlerExistsFor(typeof(RepositoryApplicationOpenedEventArgs)))
            {
                this.eventAggregator.PublishOnUIThreadAsync(new RepositoryApplicationOpenedEventArgs(applicationDescriptionViewModel.ApplicationName, applicationDescriptionViewModel.ApplicationId));
            }
            NotifyOfPropertyChange(nameof(IsApplicationOpen));
            logger.Debug("Application {Application} is open now", applicationDescriptionViewModel.ApplicationName);
        }

        public void GoBack()
        {
            IsApplicationOpen = false;
            foreach (var childView in ChildViews)
            {
                childView.SetActiveApplication(null);
            }
            if (this.eventAggregator.HandlerExistsFor(typeof(RepositoryApplicationOpenedEventArgs)))
            {
                this.eventAggregator.PublishOnUIThreadAsync(new RepositoryApplicationOpenedEventArgs(string.Empty, string.Empty));
            }
            NotifyOfPropertyChange(nameof(IsApplicationOpen));
        }       

        public async Task AddApplication(KnownApplication knownApplication)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(AddApplication), ActivityKind.Internal))
            {
                try
                {
                    Guard.Argument(knownApplication).NotNull();

                    IApplication application = (IApplication)Activator.CreateInstance(knownApplication.UnderlyingApplicationType);

                    ApplicationDescription newApplication = new ApplicationDescription(application)
                    {
                        SupportedPlatforms = knownApplication.SupportedPlatforms.ToArray()
                    };
                    var applicationDescriptionViewModel = new ApplicationDescriptionViewModel(newApplication);
                    applicationDescriptionViewModel.AddScreen("Home");
                    applicationDescriptionViewModel.ScreenCollection.SetActiveScreen("Home");
                    if (string.IsNullOrEmpty(applicationDescriptionViewModel.ApplicationName))
                    {
                        applicationDescriptionViewModel.ApplicationName = $"{this.Applications.Count() + 1}";
                        applicationDescriptionViewModel.ApplicationType = knownApplication.UnderlyingApplicationType.Name;
                    }
                    activity?.SetTag("ApplicationName", applicationDescriptionViewModel.ApplicationName);
                    activity?.SetTag("ApplicationType", applicationDescriptionViewModel.ApplicationType);

                    this.Applications.Add(applicationDescriptionViewModel);
                    this.SelectedApplication = applicationDescriptionViewModel;
                    await SaveApplicationAsync(applicationDescriptionViewModel);
                    await EditApplicationAsync(applicationDescriptionViewModel);
                    NotifyOfPropertyChange(() => Applications);
                    logger.Information("New application of type {0} has been added to the application repository", application.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to add a '{0}' application", knownApplication.ToString());
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }            
        }

        public async Task EditApplicationAsync(ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
            await this.eventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(applicationDescriptionViewModel.ApplicationDetails, 
                async () => {
                    using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(EditApplicationAsync), ActivityKind.Internal))
                    {
                        try
                        {
                            activity?.SetTag("ApplicationName", applicationDescriptionViewModel.ApplicationName);
                            await SaveApplicationAsync(applicationDescriptionViewModel);
                            await notificationManager.ShowSuccessNotificationAsync("Application was saved");
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "There was an error while trying to save application : '{0}' after edit", applicationDescriptionViewModel?.ApplicationName);
                            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                            await notificationManager.ShowErrorNotificationAsync(ex);
                        }
                    }                    
                }, 
                () => { 
                    return true; 
                }));
        }

        public async Task SaveApplicationAsync(ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
            Guard.Argument(applicationDescriptionViewModel, nameof(applicationDescriptionViewModel)).NotNull();
            await this.applicationDataManager.AddOrUpdateApplicationAsync(applicationDescriptionViewModel.Model);
            logger.Information($"Saved application data for : {applicationDescriptionViewModel.ApplicationName}");
            await this.eventAggregator.PublishOnUIThreadAsync(new ApplicationUpdatedEventArgs(applicationDescriptionViewModel.ApplicationId));
            this.Applications.Remove(applicationDescriptionViewModel);
            this.Applications.Add(applicationDescriptionViewModel);
        }

        /// <summary>
        /// Mark the application as deleted and remove from application explorer views
        /// </summary>
        /// <param name="applicationDescriptionViewModel"></param>
        /// <returns></returns>
        public async Task DeleteApplicationAsync(ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
            Guard.Argument(applicationDescriptionViewModel, nameof(applicationDescriptionViewModel)).NotNull();
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this application?", "Confirm Delete", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DeleteApplicationAsync), ActivityKind.Internal))
                {
                    try
                    {
                        activity?.SetTag("ApplicationName", applicationDescriptionViewModel.ApplicationName);
                        await this.applicationDataManager.DeleteApplicationAsync(applicationDescriptionViewModel.Model);
                        this.Applications.Remove(applicationDescriptionViewModel);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "There was an error while trying to delete application : '{0}'", applicationDescriptionViewModel?.ApplicationName);
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await notificationManager.ShowErrorNotificationAsync(ex);
                    }
                }

            }
        }

        /// <summary>
        /// Create a new screen for the application. Screens are used to group application controls.
        /// </summary>
        /// <returns></returns>
        public async Task CreateScreen(ApplicationDescriptionViewModel applicationDescription)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CreateScreen), ActivityKind.Internal))
            {
                try
                {
                    var applicationScreenViewModel = new ApplicationScreenViewModel(applicationDescription);
                    var result = await windowManager.ShowDialogAsync(applicationScreenViewModel);
                    if (result.GetValueOrDefault())
                    {
                        activity?.SetTag("ApplicationName", applicationDescription.ApplicationName);
                        activity?.SetTag("ScreenName", applicationScreenViewModel.ScreenName);
                        applicationDescription.ScreenCollection.RefreshScreens();
                        await this.applicationDataManager.AddOrUpdateApplicationAsync(applicationDescription.Model);
                        logger.Information("Added screen {0} to application {1}", applicationScreenViewModel.ScreenName, applicationDescription);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while creating new screen");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }                
        }

        /// <summary> 
        /// Rename screen to a new value
        /// </summary>
        /// <param name="selectedScreen"></param>
        /// <returns></returns>
        public async Task RenameScreen(ApplicationDescriptionViewModel applicationDescription, string screenName)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(RenameScreen), ActivityKind.Internal))
            {
                try
                {
                    var renameScreenViewModel = new RenameScreenViewModel(applicationDescription, screenName);
                    var result = await windowManager.ShowDialogAsync(renameScreenViewModel);
                    if (result.GetValueOrDefault())
                    {
                        activity?.SetTag("ApplicationName", applicationDescription.ApplicationName);
                        activity?.SetTag("CurrentScreenName", renameScreenViewModel.ScreenName);
                        activity?.SetTag("NewScreenName", renameScreenViewModel.NewScreenName);
                        applicationDescription.ScreenCollection.RefreshScreens();
                        applicationDescription.ScreenCollection.SetActiveScreen(renameScreenViewModel.NewScreenName);
                        await this.applicationDataManager.AddOrUpdateApplicationAsync(applicationDescription.Model);
                        logger.Information("Renamed screen {0} to {1} for application {2}", renameScreenViewModel.ScreenName, renameScreenViewModel.NewScreenName, applicationDescription.ApplicationName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while renaming the screen");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }            
        }

        /// <summary>

        private void InitializeKnownApplications()
        {
            var knownApps = this.typeProvider.GetKnownTypes().Where(t => t.GetInterface(nameof(IApplication)) != null);
            foreach (var knownApp in knownApps)
            {
                string displayName = TypeDescriptor.GetAttributes(knownApp).OfType<DisplayNameAttribute>()?.FirstOrDefault()?.DisplayName ?? knownApp.Name;
                string description = TypeDescriptor.GetAttributes(knownApp).OfType<DescriptionAttribute>()?.FirstOrDefault()?.Description ?? knownApp.Name;
                List<string> supportedPlatforms = TypeDescriptor.GetAttributes(knownApp).OfType<SupportedPlatformsAttribute>()?.FirstOrDefault()?.Platforms ??
                    new List<string> { OSPlatform.Windows.ToString(), OSPlatform.Linux.ToString(), OSPlatform.OSX.ToString() };
                KnownApplication knownApplication = new KnownApplication() { DisplayName = displayName, Description = description, UnderlyingApplicationType = knownApp, SupportedPlatforms = supportedPlatforms };
                this.KnownApplications.Add(knownApplication);
            }
        }

        #endregion Manage Application

        #region Edit Application       

        bool canEdit;
        public bool CanEdit
        {
            get
            {
                return canEdit;
            }
            set
            {
                canEdit = value;
                NotifyOfPropertyChange(() => CanEdit);
            }
        }

        public void ToggleRename(ApplicationDescriptionViewModel targetItem)
        {
            if (selectedApplication == targetItem)
            {
                CanEdit = !CanEdit;
            }
        }

        public async Task RenameApplication(ActionExecutionContext context, ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(RenameApplication), ActivityKind.Internal))
            {
                try
                {
                    KeyEventArgs keyArgs = context.EventArgs as KeyEventArgs;
                    if (keyArgs != null && keyArgs.Key == Key.Enter)
                    {
                        string newName = (context.Source as System.Windows.Controls.TextBox).Text;
                        if (this.Applications.Any(a => a.ApplicationName.Equals(newName)))
                        {
                            logger.Warning($"An application already exists with name {newName}.");
                            await notificationManager.ShowErrorNotificationAsync($"An application already exists with name {newName}");
                            return;
                        }
                        if (newName != applicationDescriptionViewModel.ApplicationName)
                        {
                            activity?.SetTag("CurrentApplicationName", applicationDescriptionViewModel.ApplicationName);
                            activity?.SetTag("NewApplicationName", newName);
                            var previousName = applicationDescriptionViewModel.ApplicationName;
                            applicationDescriptionViewModel.ApplicationName = newName;
                            applicationDescriptionViewModel.ApplicationDetails.ApplicationName = newName;
                            await SaveApplicationAsync(applicationDescriptionViewModel);
                            CanEdit = false;
                            logger.Information($"Application : {previousName} renamed to : {applicationDescriptionViewModel.ApplicationName}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to rename application : '{0}'", applicationDescriptionViewModel.ApplicationName);
                    CanEdit = false;
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }             
        }

        #endregion Edit Application

        #region Anchorable

        /// <summary>
        /// Preferred docked location of the view
        /// </summary>
        public override PaneLocation PreferredLocation
        {
            get
            {
                return PaneLocation.Bottom;
            }
        }

        /// <summary>
        /// Preferred height of the view
        /// </summary>
        public override double PreferredHeight
        {
            get
            {
                return 280;
            }
        }

        #endregion Anchorable        

    }
}
