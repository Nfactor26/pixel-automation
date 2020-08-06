using Caliburn.Micro;
using Dawn;
using Microsoft.Win32;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels
{
    public class HomeViewModel : ScreenBase, IHome
    {
        private readonly ILogger logger = Log.ForContext<HomeViewModel>();

        private readonly ISerializer serializer;
        private readonly IWindowManager windowManager;     
        private readonly IApplicationDataManager applicationDataManager;

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

        public HomeViewModel(ISerializer serializer, IWindowManager windowManager, IApplicationDataManager applicationDataManager)
        {
            this.DisplayName = "Home";
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;           
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
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
                        case ".proc":
                            string projectFileName = Path.GetFileNameWithoutExtension(fileToOpen);
                            automationProject = serializer.Deserialize<AutomationProject>($"Automations\\{projectFileName}\\{projectFileName}.atm", null);
                            break;
                    }

                }

                //we need a new instance of automationEditor everytime we open a project. Don't inject it as a constructor parameter.
                //TODO : Create a factory for automation editor which should be injected as a constructor parameter.
                var automationEditor = IoC.Get<IAutomationBuilder>();
                await automationEditor.DoLoad(automationProject);   
                if(this.Parent is IConductor conductor)
                {
                    await conductor.ActivateItemAsync(automationEditor as Screen);
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Automation Project (*.atm)|*.atm|Process Files(*.proc)|*.proc";
            openFileDialog.InitialDirectory = this.applicationDataManager.GetProjectsRootDirectory();
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

        #endregion Close Screen
    }
}
