using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Serilog;
using System.ComponentModel;
using System.Windows.Data;

namespace Pixel.Automation.Designer.ViewModels
{
    /// <summary>
    /// Screen for showing available projects and creating new projects when the application is launched
    /// </summary>
    public class HomeViewModel : ScreenBase, IHome, IHandle<EditorClosedNotification>
    {
        private readonly ILogger logger = Log.ForContext<HomeViewModel>();

        private readonly IEventAggregator eventAggregator;
        private readonly ISerializer serializer;
        private readonly IWindowManager windowManager;     
        private readonly IApplicationDataManager applicationDataManager;
        private readonly IApplicationFileSystem fileSystem;
        
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

        public HomeViewModel(IEventAggregator eventAggregator, ISerializer serializer, IWindowManager windowManager, IApplicationDataManager applicationDataManager,
            IApplicationFileSystem fileSystem)
        {
            this.DisplayName = "Home";
            this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
            this.eventAggregator.SubscribeOnBackgroundThread(this);
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;       
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem, nameof(fileSystem)).NotNull().Value;
            LoadProjects();
            CreateCollectionView();
        }

        private void LoadProjects()
        {
            var availableProjects = this.applicationDataManager.GetAllProjects();
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
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error trying to create project.");               
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
               
                logger.Information($"Trying to open project : {automationProject.Name}");

                var editorFactory = IoC.Get<IEditorFactory>();
                var automationEditor = editorFactory.CreateAutomationEditor();
                await automationEditor.DoLoad(automationProject, automationProjectViewModel.SelectedVersion);
                if (this.Parent is IConductor conductor)
                {
                    await conductor.ActivateItemAsync(automationEditor as Caliburn.Micro.Screen);
                    automationProjectViewModel.IsOpenInEditor = true;
                    logger.Information($"Project : {automationProject.Name} is open now.");
                    return;
                }
                throw new Exception("Failed to activate automation editor after loading project");           

            }
            catch (Exception ex)
            {               
                logger.Error(ex, "There was an error trying to open project.");
                automationProjectViewModel.IsOpenInEditor = false;
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
        public async Task HandleAsync(EditorClosedNotification message, CancellationToken cancellationToken)
        {
            try
            {
                var project = this.Projects.FirstOrDefault(a => a.ProjectId.Equals(message.AutomationProject.ProjectId));
                if(project != null)
                {
                    project.IsOpenInEditor = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
            await Task.CompletedTask;
        }

        #endregion Close Screen
    }
}
