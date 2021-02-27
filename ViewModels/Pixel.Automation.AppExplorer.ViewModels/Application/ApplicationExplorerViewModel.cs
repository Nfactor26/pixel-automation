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
    public class ApplicationExplorerViewModel : Conductor<IScreen>.Collection.OneActive, IToolBox, IDisposable
    {
        private readonly ILogger logger = Log.ForContext<ApplicationExplorerViewModel>();
    
        private readonly IEventAggregator eventAggregator;      
        private readonly ITypeProvider typeProvider;   
        private readonly IApplicationDataManager applicationDataManager;

        public ControlExplorerViewModel ControlExplorer { get; private set; }

        public PrefabExplorerViewModel PrefabExplorer { get; private set; }

        public BindableCollection<ApplicationDescription> Applications { get; set; } = new BindableCollection<ApplicationDescription>();

        public BindableCollection<KnownApplication> KnownApplications { get; set; } = new BindableCollection<KnownApplication>();

        private ApplicationDescription selectedApplication;
        public ApplicationDescription SelectedApplication
        {
            get => selectedApplication;
            set
            {
                selectedApplication = value;
                NotifyOfPropertyChange(() => SelectedApplication);
                CanEdit = false;
                if (value != null)
                {
                    //If we directly click an application icon without selecting view first, IsActiveItem is not set. Hence, explicitly setting it whenever
                    //one of the application is selected.
                    this.IsActiveItem = true;
                    //Notification for property grid to display selected application details
                    this.eventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(value.ApplicationDetails));
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
            this.Applications.AddRange(applications);
            Applications.OrderBy(a => a.ApplicationName);
        }      

        private void CreateCollectionView()
        {
            var groupedItems = CollectionViewSource.GetDefaultView(Applications);
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ApplicationDescription.ApplicationName)));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(ApplicationDescription.ApplicationName), ListSortDirection.Ascending));
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                return (a as ApplicationDescription).ApplicationName.ToLower().Contains(this.filterText.ToLower());
            });
        }

        private void OnPrefabCreated(object sender, PrefabDescription e)
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

        public void OpenApplication(ApplicationDescription application)
        {
            Guard.Argument(application).NotNull();

            IsApplicationOpen = true;
            ControlExplorer.SetActiveApplication(application);
            PrefabExplorer.SetActiveApplication(application);
            if (this.eventAggregator.HandlerExistsFor(typeof(RepositoryApplicationOpenedEventArgs)))
            {
                this.eventAggregator.PublishOnUIThreadAsync(new RepositoryApplicationOpenedEventArgs(application.ApplicationName, application.ApplicationId));
            }
            NotifyOfPropertyChange(nameof(IsApplicationOpen));
        }
       
        public void GoBack()
        {
            IsApplicationOpen = false;        
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
            if (string.IsNullOrEmpty(newApplication.ApplicationName))
            {
                newApplication.ApplicationName = $"{this.Applications.Count() + 1}";
                newApplication.ApplicationType = knownApplication.UnderlyingApplicationType.Name;
            }
        
            this.Applications.Add(newApplication);
            this.SelectedApplication = newApplication;
            await SaveApplicationAsync(newApplication);
            NotifyOfPropertyChange(() => Applications);
           
            logger.Information($"New application of type {application.ToString()} has been added to the application repository");
        }

        public async Task SaveApplicationAsync(ApplicationDescription application)
        {
            Guard.Argument(application).NotNull();
            await this.applicationDataManager.AddOrUpdateApplicationAsync(application);
            logger.Information($"Saved application data for : {application.ApplicationName}");
            await this.eventAggregator.PublishOnBackgroundThreadAsync(new ApplicationUpdatedEventArgs(application.ApplicationId));           
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

        public void ToggleRename(ApplicationDescription targetItem)
        {
            if (selectedApplication == targetItem)
            {
                CanEdit = !CanEdit;
            }
        }

        public async Task RenameApplication(ActionExecutionContext context, ApplicationDescription application)
        {
            try
            {
                KeyEventArgs keyArgs = context.EventArgs as KeyEventArgs;
                if (keyArgs != null && keyArgs.Key == Key.Enter)
                {
                    string newName = (context.Source as System.Windows.Controls.TextBox).Text;
                    if (newName != application.ApplicationName)
                    {
                        var previousName = application.ApplicationName;
                        application.ApplicationName = newName;
                        application.ApplicationDetails.ApplicationName = newName;
                        await SaveApplicationAsync(application);
                        CanEdit = false;
                        logger.Information($"Application : {previousName} renamed to : {application.ApplicationName}");
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

        #region IToolBox

        bool isActiveItem;
        public bool IsActiveItem
        {
            get => isActiveItem;
            set
            {
                isActiveItem = value;              
                NotifyOfPropertyChange(() => IsActiveItem);
                if (isActiveItem == false)
                {
                    this.SelectedApplication = null;
                }
            }

        }

        public PaneLocation PreferredLocation
        {
            get
            {
                return PaneLocation.Bottom;
            }
        }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                NotifyOfPropertyChange(() => IsVisible);
            }
        }

        public double PreferredWidth
        {
            get
            {
                return 250;
            }
        }

        public double PreferredHeight
        {
            get
            {
                return 280;
            }
        }

        protected ICommand closeCommand;
        public virtual ICommand CloseCommand
        {
            get { return closeCommand ?? (closeCommand = new RelayCommand<bool>(p => IsVisible = false, p => true)); }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        #endregion IToolBox        

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
