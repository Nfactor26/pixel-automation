using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Notifications.Notfications;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Data;
using Pixel.Automation.Editor.Core.Helpers;

namespace Pixel.Automation.Designer.ViewModels
{
    /// <summary>
    /// Screen for showing available projects and creating new projects when the application is launched
    /// </summary>
    public class HomeViewModel : ScreenBase, IHome, IHandle<EditorClosedNotification<AutomationProject>>, IHandle<OpenProjectVersionForEditNotification>
    {
        private readonly ILogger logger = Log.ForContext<HomeViewModel>();

        private readonly IEventAggregator eventAggregator;
        private readonly ISerializer serializer;
        private readonly IWindowManager windowManager;            
        private readonly IProjectDataManager projectDataManager;      
        private readonly IVersionManagerFactory versionManagerFactory;
        private readonly IApplicationFileSystem applicationFileSystem;
        private readonly INotificationManager notificationManager;

        /// <summary>
        /// Collection of availalbe projects
        /// </summary>
        public BindableCollection<AutomationProjectViewModel> Projects { get; private set; } = new ();


        string filterText = string.Empty;
        /// <summary>
        /// Filter available projects using filter text
        /// </summary>
        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;
                var view = CollectionViewSource.GetDefaultView(this.Projects);
                view.Refresh();
                NotifyOfPropertyChange(() => this.Projects);

            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="serializer"></param>
        /// <param name="windowManager"></param>
        /// <param name="applicationDataManager"></param>
        /// <param name="fileSystem"></param>

        public HomeViewModel(IEventAggregator eventAggregator, ISerializer serializer, IWindowManager windowManager,
            IProjectDataManager projectDataManager, IVersionManagerFactory versionManagerFactory, 
            IApplicationFileSystem applicationFileSystem, INotificationManager notificationManager)
        {
            this.DisplayName = "Home";
            this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
            this.eventAggregator.SubscribeOnUIThread(this);
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;           
            this.projectDataManager = Guard.Argument(projectDataManager, nameof(projectDataManager)).NotNull().Value;           
            this.versionManagerFactory = Guard.Argument(versionManagerFactory, nameof(versionManagerFactory)).NotNull().Value;
            this.applicationFileSystem = Guard.Argument(applicationFileSystem, nameof(applicationFileSystem)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;

            LoadProjects();
            CreateCollectionView();
        }

        private void LoadProjects()
        {
            var availableProjects = this.projectDataManager.GetAllProjects();
            this.Projects.AddRange(availableProjects.Select(p => new AutomationProjectViewModel(p)));          
        }

        private void CreateCollectionView()
        {
            var groupedItems = CollectionViewSource.GetDefaultView(this.Projects);        
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(AutomationProjectViewModel.Name), ListSortDirection.Ascending));
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                return (a as AutomationProjectViewModel).Name.ToLower().Contains(this.filterText.ToLower());
            });
        }

        /// <summary>
        /// Create a new automation project
        /// </summary>
        /// <returns></returns>
        public async Task CreateNewProject()
        {
            using(var activity = Telemetry.DefaultSource?.StartActivity(nameof(CreateNewProject), ActivityKind.Internal))
            {
                try
                {
                    INewProject newProjectVM = IoC.Get<INewProject>();
                    AutomationProject newProject = newProjectVM.NewProject;
                    var result = await windowManager.ShowDialogAsync(newProjectVM);
                    if (result.HasValue && result.Value)
                    {
                        var newProjectViewModel = new AutomationProjectViewModel(newProject);
                        this.Projects.Insert(0, newProjectViewModel);
                        await OpenProject(newProjectViewModel);
                        var projectsDirectory = this.applicationFileSystem.GetAutomationProjectWorkingDirectory(newProject, newProject.LatestActiveVersion);
                        await this.projectDataManager.AddOrUpdateDataFileAsync(newProject, newProject.LatestActiveVersion,
                            Path.Combine(projectsDirectory, Constants.AutomationProcessFileName), newProject.ProjectId);
                        await this.projectDataManager.AddOrUpdateDataFileAsync(newProject, newProject.LatestActiveVersion,
                          Path.Combine(projectsDirectory, Constants.DataModelDirectory, $"{Constants.AutomationProcessDataModelName}.cs"), newProject.ProjectId);
                        await this.projectDataManager.AddOrUpdateDataFileAsync(newProject, newProject.LatestActiveVersion,
                         Path.Combine(projectsDirectory, Constants.ScriptsDirectory, Constants.InitializeEnvironmentScript), newProject.ProjectId);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error trying to create project.");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }           
        }

        private readonly object locker = new();
        /// <summary>
        /// Open an existng automation project
        /// </summary>
        /// <param name="automationProjectViewModel"></param>
        /// <returns></returns>
        public async Task OpenProject(AutomationProjectViewModel automationProjectViewModel)
        {
            await OpenProject(automationProjectViewModel, automationProjectViewModel.SelectedVersion);        
        }


        private async Task OpenProject(AutomationProjectViewModel automationProjectViewModel, ProjectVersion projectVersion)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(OpenProject), ActivityKind.Internal))
            {
                try
                {
                    Guard.Argument(automationProjectViewModel, nameof(automationProjectViewModel)).NotNull();
                    AutomationProject automationProject = automationProjectViewModel.Project;
                    lock (locker)
                    {
                        if (automationProjectViewModel.IsOpenInEditor)
                        {
                            logger.Information($"Project {automationProject.Name} is already open.");
                            return;
                        }
                        automationProjectViewModel.IsOpenInEditor = true;
                    }

                    logger.Information("Trying to open version : '{0}' of project : '{1}'", projectVersion, automationProject.Name);

                    var editorFactory = IoC.Get<IEditorFactory>();
                    var automationEditor = editorFactory.CreateAutomationEditor();
                    await automationEditor.DoLoad(automationProject, projectVersion);
                    if (this.Parent is IConductor conductor)
                    {
                        await conductor.ActivateItemAsync(automationEditor as Caliburn.Micro.Screen);
                        automationProjectViewModel.IsOpenInEditor = true;
                        logger.Information("Version : '{0}' of Project : '{1}' is open now", projectVersion, automationProject.Name);
                        return;
                    }

                }
                catch (Exception ex)
                {
                    automationProjectViewModel.IsOpenInEditor = false;
                    logger.Error(ex, "There was an error trying to open version : '{0}' of  project : '{1}'", projectVersion, automationProjectViewModel.Name);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
        }

        /// <summary>
        /// Open manager window to manage available version of the automation project
        /// </summary>
        /// <param name="automationProjectViewModel"></param>
        /// <returns></returns>
        public async Task ManageProjectVersionAsync(AutomationProjectViewModel automationProjectViewModel)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(ManageProjectVersionAsync), ActivityKind.Internal))
            {
                try
                {
                    var versionManager = this.versionManagerFactory.CreateProjectVersionManager(automationProjectViewModel.Project);
                    var result = await this.windowManager.ShowDialogAsync(versionManager);
                    if (result.HasValue && result.Value)
                    {
                        automationProjectViewModel.RefreshVersions();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, ex.Message);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }           
        }


        #region Close Screen

        /// <summary>
        /// Indicate whether the screen can be closed
        /// </summary>
        /// <returns></returns>
        public override bool CanClose()
        {
            return true;
        }

        /// <inheritdoc/>    
        public override async void CloseScreen()
        {           
            await this.TryCloseAsync(true);        
            logger.Information($"{nameof(HomeViewModel)} was closed.");
        }

        /// <summary>
        /// Notificaton handler for <see cref="EditorClosedNotification"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(EditorClosedNotification<AutomationProject> message, CancellationToken cancellationToken)
        {
            try
            {
                var project = this.Projects.FirstOrDefault(a => a.ProjectId.Equals(message.Project.ProjectId));
                if(project != null)
                {
                    project.IsOpenInEditor = false;                  
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Notification handler for <see cref="OpenProjectVersionForEditNotification"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(OpenProjectVersionForEditNotification message, CancellationToken cancellationToken)
        {
            try
            {
                var project = this.Projects.FirstOrDefault(p => p.ProjectId.Equals(message.AutomationProject.ProjectId));
                if (project != null)
                {
                    await OpenProject(project, message.VersionToOpen);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }       
        }

        #endregion Close Screen
    }
}
