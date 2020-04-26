using Caliburn.Micro;
using Microsoft.Win32;
using Pixel.Automation.Core.Args;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Designer.ViewModels.Flyouts;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;

namespace Pixel.Automation.Designer.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell, IHandle<PropertyGridObjectEventArgs>, IHandle<ActivateScreenNotification>
    {
        public IObservableCollection<FlyoutBaseViewModel> FlyoutViewModels { get; }
            = new BindableCollection<FlyoutBaseViewModel>();

        public BindableCollection<IToolBox> Tools { get; }

        public List<IControlScrapper> ScreenScrappers { get; }

        ISerializer serializer;

        public ShellViewModel(IEventAggregator eventAggregator, ISerializer serializer, List<IToolBox> tools, List<IControlScrapper> scrappers) : base()
        {
            LogManager.GetLog = t => new DebugLog(t);
            DisplayName = "Pixel Automation";

            this.Tools = new BindableCollection<IToolBox>();
            this.Tools.AddRange(tools);
            propertyGrid = this.Tools.OfType<PropertyGridViewModel>().FirstOrDefault();


            this.ScreenScrappers = scrappers;
            this.serializer = serializer;
            eventAggregator.SubscribeOnUIThread(this);

            _ = ActivateItemAsync(IoC.Get<IHome>() as Screen, CancellationToken.None);

            this.FlyoutViewModels.Add(new SettingsViewModel());
        }

        public void ToggleFlyout(int index)
        {
            var flyout = this.FlyoutViewModels[index];
            flyout.IsOpen = !flyout.IsOpen;
        }

        public async void DoNew()
        {
            Log.Debug("DoNew start");
            if (!Directory.Exists("Automations"))
            {
                Directory.CreateDirectory("Automations");
                Log.Information("Created folder Automations");
            }

            IWindowManager windowManager = IoC.Get<IWindowManager>();
            INewProject newProjectVM = IoC.Get<INewProject>();
            AutomationProject newProject = newProjectVM.NewProject;
            var result = await windowManager.ShowDialogAsync(newProjectVM);
            if (result.HasValue && result.Value)
            {
                OpenProject(newProject);
            }
            Log.Debug("DoNew end");
        }

        public void DoOpen()
        {
            Log.Debug("DoOpen start");

            var fileToOpen = ShowOpenFileDialog();
            Log.Information("Picked project file : {$file} to open", fileToOpen);
            if (string.IsNullOrEmpty(fileToOpen))
                return;
            var automationProject = serializer.Deserialize<AutomationProject>(fileToOpen, null);
            OpenProject(automationProject);
            Log.Debug("DoOpen end");

        }

        private async void OpenProject(AutomationProject automationProject)
        {
            Log.Debug("OpenProject start");

            var automationBuilder = IoC.Get<IAutomationBuilder>();
            var shell = IoC.Get<IShell>();
            await (shell as ShellViewModel).ActivateItemAsync(automationBuilder as Screen);
            automationBuilder.DoLoad(automationProject);

            Log.Debug("OpenProject end");

        }

        private string ShowOpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Automation Project (*.atm)|*.atm";
            openFileDialog.InitialDirectory = "Automations";
            if (openFileDialog.ShowDialog() == true)
                return openFileDialog.FileName;
            return string.Empty;
        }


        public bool CanSave
        {
            get
            {
                return this.ActiveItem != null && (this.ActiveItem is IEditor);
            }
        }

        public void Save()
        {
            //TODO : Having run a processor , trying to save process freezes on attempting to log. Resolve this issue.

            Log.Debug("Save start");

            if (this.ActiveItem != null)
            {
                if (this.ActiveItem is IEditor editor)
                {
                    editor.DoSave();
                }
            }

            Log.Debug("Save end");

        }


        public bool CanSaveAll
        {
            get
            {
                return this.Items.Any(i => i is IEditor);
            }
        }

        public void SaveAll()
        {
            Log.Debug("SaveAll start");

            foreach (var item in this.Items)
            {
                if (item is IEditor editor)
                {
                    editor.DoSave();
                }
            }

            Log.Debug("SaveAll end");
        }

        public bool CanEditDataModel
        {
            get
            {
                return this.ActiveItem != null && this.ActiveItem is IEditor;
            }
        }

        public void EditDataModel()
        {
            var activeItem = this.ActiveItem as IEditor;
            if (activeItem == null)
                return;
            activeItem.EditDataModel();
        }


        public bool CanReloadAutomationProject
        {
            get
            {
                return this.ActiveItem is IAutomationBuilder;
            }

        }
        public void ReloadAutomationProject()
        {
            var activeItem = this.ActiveItem as IAutomationBuilder;
            if (activeItem != null)
            {
                activeItem.DoSave();
                activeItem.DoUnload();
                activeItem.DoLoad(activeItem.CurrentProject);
            }
        }

        public bool CanManage
        {
            get
            {
                return this.ActiveItem is IEditor;
            }

        }

        public void Manage()
        {
            var activeItem = this.ActiveItem as IEditor;
            if (activeItem != null)
            {
                activeItem.Manage();
            }
        }

        public override async Task ActivateItemAsync(IScreen item, CancellationToken cancellationToken)
        {
            //Temp fix
            //TODO : Check why selecting a tool box item causes ActivateItem to be triggered by avalon dock
            if (item is IToolBox)
            {
                return;
            }

            try
            {
                await base.ActivateItemAsync(item, cancellationToken);
                NotifyOfPropertyChange(() => CanEditDataModel);
                NotifyOfPropertyChange(() => CanSave);
                NotifyOfPropertyChange(() => CanSaveAll);
                NotifyOfPropertyChange(() => CanManage);
                NotifyOfPropertyChange(() => CanReloadAutomationProject);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            } 

        }


        #region Manage Windows

        public void ShowToolBox(string toolBoxName)
        {
            var targetToolBox = this.Tools.FirstOrDefault(t => t.DisplayName.Equals(toolBoxName));
            if (targetToolBox != null)
            {
                targetToolBox.IsActiveItem = true;
                targetToolBox.IsVisible = true;
                targetToolBox.IsSelected = true;
            }
        }

        public void ShowStartPage()
        {
            _ = ActivateItemAsync(IoC.Get<IHome>() as Screen, CancellationToken.None);
        }

        #endregion Manage Windows

        #region property grid

        private PropertyGridViewModel propertyGrid;

        public async Task HandleAsync(PropertyGridObjectEventArgs message, CancellationToken cancellationToken)
        {
            propertyGrid.SelectedObject = message.ObjectToDisplay;
            propertyGrid.IsReadOnly = message.IsReadOnly;
            await Task.CompletedTask;
        }

        #endregion property grid

        public async Task HandleAsync(ActivateScreenNotification activateScreenNotification, CancellationToken cancellationToken)
        {
            if (activateScreenNotification?.ScreenToActivate != null)
            {
                var shell = IoC.Get<IShell>();
                await (shell as ShellViewModel).ActivateItemAsync(activateScreenNotification.ScreenToActivate, CancellationToken.None);
            }
            await Task.CompletedTask;
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            ITestExplorer testExplorer = this.Tools.OfType<ITestExplorer>().FirstOrDefault();
            MessageBoxResult result = default;
            if (testExplorer.HasTestCaseOpenForEdit())
            {
                result = MessageBox.Show("Test explorer has unsaved changed. Are you sure you want to exit?",
                    "Confirm Quit", MessageBoxButton.OKCancel);             
            }
            else
            {
                result = MessageBox.Show("Are you sure you want to exit?",
                 "Confirm Quit", MessageBoxButton.OKCancel);            
            }
            if (result == MessageBoxResult.Cancel)
            {
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        
    }
}
