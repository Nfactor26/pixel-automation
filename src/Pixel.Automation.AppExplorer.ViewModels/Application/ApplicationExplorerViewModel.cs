using Caliburn.Micro;
using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core.Args;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Persistence.Services.Client;
using Serilog;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Pixel.Automation.AppExplorer.ViewModels.Application
{
    public class ApplicationExplorerViewModel : AnchorableHost, IDisposable
    {
        private readonly ILogger logger = Log.ForContext<ApplicationExplorerViewModel>();
    
        private readonly IEventAggregator eventAggregator;      
        private readonly ITypeProvider typeProvider;   
        private readonly IApplicationDataManager applicationDataManager;

        public ControlExplorerViewModel ControlExplorer { get; private set; }

        public PrefabExplorerViewModel PrefabExplorer { get; private set; }

        public BindableCollection<ApplicationDescriptionViewModel> Applications { get; set; } = new BindableCollection<ApplicationDescriptionViewModel>();

        public BindableCollection<KnownApplication> KnownApplications { get; set; } = new BindableCollection<KnownApplication>();

        private ApplicationDescriptionViewModel selectedApplication;
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

        public ApplicationExplorerViewModel(IEventAggregator eventAggregator, IApplicationDataManager applicationDataManager, ITypeProvider typeProvider,
             ControlExplorerViewModel controlExplorer, PrefabExplorerViewModel prefabExplorer)
        {
            this.DisplayName = "Application Repository";
            this.eventAggregator = eventAggregator;
            this.typeProvider = typeProvider;          
            this.applicationDataManager = applicationDataManager;
            this.ControlExplorer = controlExplorer;
            this.PrefabExplorer = prefabExplorer;           
            this.PrefabExplorer.PrefabCreated += OnPrefabCreated;           
            CreateCollectionView();
            InitializeKnownApplications();
            
            var applications = this.applicationDataManager.GetAllApplications();
            foreach(var application in applications)
            {
                this.Applications.Add(new ApplicationDescriptionViewModel(application));
            }
            Applications.OrderBy(a => a.ApplicationName);
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

        private void OnPrefabCreated(object sender, PrefabProject e)
        {
            try
            {
                var targetApplication = this.Applications.Where(a => a.ApplicationId.Equals(e.ApplicationId)).FirstOrDefault();
                targetApplication.AddPrefab(e);
                _ = SaveApplicationAsync(targetApplication);
                logger.Information($"Added Prefab {e.PrefabName} to application : {targetApplication.ApplicationName}");
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

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

        #endregion Filter

        #region Manage Application

        public bool IsApplicationOpen { get; set; }

        public void OpenApplication(ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
            Guard.Argument(applicationDescriptionViewModel).NotNull();

            IsApplicationOpen = true;
            ControlExplorer.SetActiveApplication(applicationDescriptionViewModel);
            PrefabExplorer.SetActiveApplication(applicationDescriptionViewModel);
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
            ControlExplorer.SetActiveApplication(null);
            PrefabExplorer.SetActiveApplication(null);
            if (this.eventAggregator.HandlerExistsFor(typeof(RepositoryApplicationOpenedEventArgs)))
            {
                this.eventAggregator.PublishOnUIThreadAsync(new RepositoryApplicationOpenedEventArgs(string.Empty, string.Empty));
            }
            NotifyOfPropertyChange(nameof(IsApplicationOpen));
        }

        public async Task AddApplication(KnownApplication knownApplication)
        {
            Guard.Argument(knownApplication).NotNull();

            IApplication application = (IApplication)Activator.CreateInstance(knownApplication.UnderlyingApplicationType);

            ApplicationDescription newApplication = new ApplicationDescription(application);
            var applicationDescriptionViewModel = new ApplicationDescriptionViewModel(newApplication);
            if (string.IsNullOrEmpty(applicationDescriptionViewModel.ApplicationName))
            {
                applicationDescriptionViewModel.ApplicationName = $"{this.Applications.Count() + 1}";
                applicationDescriptionViewModel.ApplicationType = knownApplication.UnderlyingApplicationType.Name;
            }
           
            this.Applications.Add(applicationDescriptionViewModel);
            this.SelectedApplication = applicationDescriptionViewModel;
            await SaveApplicationAsync(applicationDescriptionViewModel);
            await EditApplicationAsync(applicationDescriptionViewModel);
            NotifyOfPropertyChange(() => Applications);
            logger.Information("New application of type {0} has been added to the application repository", application.ToString());
        }

        public async Task EditApplicationAsync(ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
           await this.eventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(applicationDescriptionViewModel.ApplicationDetails, () => { _ = SaveApplicationAsync(applicationDescriptionViewModel); } , () => { return true; } ) );
        }

        public async Task SaveApplicationAsync(ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
            Guard.Argument(applicationDescriptionViewModel).NotNull();
            await this.applicationDataManager.AddOrUpdateApplicationAsync(applicationDescriptionViewModel.Model);
            logger.Information($"Saved application data for : {applicationDescriptionViewModel.ApplicationName}");
            await this.eventAggregator.PublishOnBackgroundThreadAsync(new ApplicationUpdatedEventArgs(applicationDescriptionViewModel.ApplicationId));
            this.Applications.Remove(applicationDescriptionViewModel);
            this.Applications.Add(applicationDescriptionViewModel);
        }

        private void InitializeKnownApplications()
        {
            var knownApps = this.typeProvider.GetAllTypes().Where(t => t.GetInterface(nameof(IApplication)) != null);
            foreach(var knownApp in knownApps)
            {
                string displayName = TypeDescriptor.GetAttributes(knownApp).OfType<DisplayNameAttribute>()?.FirstOrDefault()?.DisplayName ?? knownApp.Name;
                string description = TypeDescriptor.GetAttributes(knownApp).OfType<DescriptionAttribute>()?.FirstOrDefault()?.Description ?? knownApp.Name;
                KnownApplication knownApplication = new KnownApplication() { DisplayName = displayName, Description = description, UnderlyingApplicationType = knownApp };
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
            try
            {
                KeyEventArgs keyArgs = context.EventArgs as KeyEventArgs;
                if (keyArgs != null && keyArgs.Key == Key.Enter)
                {
                    string newName = (context.Source as System.Windows.Controls.TextBox).Text;
                    if(this.Applications.Any(a => a.ApplicationName.Equals(newName)))
                    {
                        logger.Warning($"An application already exists with name {newName}.");
                        return;
                    }
                    if (newName != applicationDescriptionViewModel.ApplicationName)
                    {
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
                logger.Error(ex, ex.Message);
                CanEdit = false;
            }
        }

        #endregion Edit Application

        #region Anchorable
        
        public override PaneLocation PreferredLocation
        {
            get
            {
                return PaneLocation.Bottom;
            }
        }
      
        public override double PreferredHeight
        {
            get
            {
                return 280;
            }
        }

        #endregion Anchorable        

        #region IDisposable

        protected virtual void Dispose(bool isDisposing)
        {           
            this.PrefabExplorer.PrefabCreated -= OnPrefabCreated;           
            logger.Information($"{nameof(ApplicationExplorerViewModel)} has been disposed");
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable
               
    }
}
