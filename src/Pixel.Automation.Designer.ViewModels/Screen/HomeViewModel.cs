using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Serilog;
using System.IO;

namespace Pixel.Automation.Designer.ViewModels
{
    public class HomeViewModel : ScreenBase, IHome, IHandle<EditorClosedNotification>
    {
        private readonly ILogger logger = Log.ForContext<HomeViewModel>();

        private readonly IEventAggregator eventAggregator;
        private readonly ISerializer serializer;
        private readonly IWindowManager windowManager;     
        private readonly IApplicationDataManager applicationDataManager;
        private readonly IApplicationFileSystem fileSystem;
        private readonly List<AutomationProject> openProjects = new List<AutomationProject>();

        BindableCollection<AutomationProject> recentProjects = new BindableCollection<AutomationProject>();
        public BindableCollection<AutomationProject> RecentProjects
        {
            get
            {
                return recentProjects;
            }
            set
            {
                recentProjects = value;
                NotifyOfPropertyChange(() => RecentProjects);
            }
        }


        public HomeViewModel(IEventAggregator eventAggregator, ISerializer serializer, IWindowManager windowManager, IApplicationDataManager applicationDataManager,
            IApplicationFileSystem fileSystem)
        {
            this.DisplayName = "Home";
            this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
            this.eventAggregator.SubscribeOnBackgroundThread(this);
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;       
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem).NotNull().Value;
            LoadRecentProjects();
        }

        private void LoadRecentProjects()
        {
            var availableProjects = this.applicationDataManager.GetAllProjects();           
            int count = availableProjects.Count() > 5 ? 5 : availableProjects.Count();
            this.RecentProjects.AddRange(availableProjects.OrderBy(a => a.LastOpened).Take(count));

        }

        public async Task CreateNewProject()
        {
            INewProject newProjectVM = IoC.Get<INewProject>();
            AutomationProject newProject = newProjectVM.NewProject;
            var result = await windowManager.ShowDialogAsync(newProjectVM);
            if (result.HasValue && result.Value)
            {
               await OpenProject(newProject);
            }
        }

        public async Task OpenProject(AutomationProject automationProject)
        {
            try
            {
                if (automationProject == null)
                {
                    var fileToOpen = ShowOpenFileDialog();
                    if (string.IsNullOrEmpty(fileToOpen))
                    {
                        return;
                    }

                    string fileType = Path.GetExtension(fileToOpen);
                    switch (fileType)
                    {
                        case ".atm":
                            automationProject = serializer.Deserialize<AutomationProject>(fileToOpen, null);
                            break;                    
                    }
                }

                if(this.openProjects.Any(a => a.ProjectId.Equals(automationProject.ProjectId)))
                {
                    logger.Information($"Project {automationProject.Name} is already open.");
                    return;
                }

                logger.Information($"Trying to open project : {automationProject.Name}");

                var editorFactory = IoC.Get<IEditorFactory>();
                var automationEditor = editorFactory.CreateAutomationEditor();
                await automationEditor.DoLoad(automationProject);   
                if(this.Parent is IConductor conductor)
                {
                    await conductor.ActivateItemAsync(automationEditor as Caliburn.Micro.Screen);
                    this.openProjects.Add(automationProject);
                    logger.Information($"Project : {automationProject.Name} is open now.");
                    return;
                }
                throw new Exception("Failed to activate automation editor after loading project");

            }
            catch (Exception ex)
            {
                logger.Warning($"There was an error while trying to open project : {automationProject.Name}");
                logger.Error(ex, ex.Message);            
            }
        }

        private string ShowOpenFileDialog()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Automation Project (*.atm)|*.atm";
            openFileDialog.InitialDirectory = this.fileSystem.GetAutomationsDirectory();
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return string.Empty;
        }

        #region Close Screen

        public override bool CanClose()
        {
            return true;
        }

        public override async void CloseScreen()
        {           
            await this.TryCloseAsync(true);        
            logger.Information($"{nameof(HomeViewModel)} was closed.");
        }

        public async Task HandleAsync(EditorClosedNotification message, CancellationToken cancellationToken)
        {
            try
            {
                var project = this.openProjects.FirstOrDefault(a => a.ProjectId.Equals(message.AutomationProject.ProjectId));
                if (project != null)
                {
                    this.openProjects.Remove(project);
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
