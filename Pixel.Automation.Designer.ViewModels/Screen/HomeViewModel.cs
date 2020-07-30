using Caliburn.Micro;
using Dawn;
using Microsoft.Win32;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels
{
    public class HomeViewModel : ScreenBase, IHome
    {
        ISerializer serializer;

        BindableCollection<AutomationProject> recentProjects;
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

        public HomeViewModel(ISerializer serializer)
        {
            Guard.Argument(serializer).NotNull($"{nameof(serializer)} is reuired parameter");

            this.DisplayName = "Home";
            this.serializer = serializer;
            LoadRecentProjects();
        }

        private void LoadRecentProjects()
        {
            if (!Directory.Exists("Automations"))
            {
                Directory.CreateDirectory("Automations");
            }
            List<AutomationProject> automationProjects = new List<AutomationProject>();
            foreach (var item in Directory.EnumerateDirectories("Automations"))
            {
                string automationProjectFile = $"{item}\\{Path.GetFileName(item)}.atm";
                var automationProject = serializer.Deserialize<AutomationProject>(automationProjectFile, null);
                automationProjects.Add(automationProject);
            }

            this.RecentProjects = new BindableCollection<AutomationProject>();
            int count = automationProjects.Count() > 5 ? 5 : automationProjects.Count();
            this.RecentProjects.AddRange(automationProjects.OrderBy(a => a.LastOpened).Take(count));

        }

        public async Task CreateNewProject()
        {
            if (!Directory.Exists("Automations"))
            {
                Directory.CreateDirectory("Automations");
            }

            IWindowManager windowManager = IoC.Get<IWindowManager>();
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

                IAutomationBuilder automationBuilder = IoC.Get<IAutomationBuilder>();
                await automationBuilder.DoLoad(automationProject);
                var shell = IoC.Get<IShell>();
                await (shell as ShellViewModel).ActivateItemAsync(automationBuilder as Screen);

            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);            
            }
        }

        private string ShowOpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Automation Project (*.atm)|*.atm|Process Files(*.proc)|*.proc";
            openFileDialog.InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Automations");
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
            var shell = IoC.Get<IShell>();
            await this.TryCloseAsync(true);
            await (shell as ShellViewModel).DeactivateItemAsync(this, true, CancellationToken.None);
        }

        #endregion Close Screen
    }
}
