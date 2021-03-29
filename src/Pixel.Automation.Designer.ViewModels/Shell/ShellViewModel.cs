using Caliburn.Micro;
using Dawn;
using Microsoft.Win32;
using Pixel.Automation.Core.Args;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Pixel.Automation.Designer.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell, IHandle<PropertyGridObjectEventArgs>, IHandle<ActivateScreenNotification>
    {
        private readonly ILogger logger = Log.ForContext<ShellViewModel>();

        public IObservableCollection<IFlyOut> FlyoutViewModels { get; }
            = new BindableCollection<IFlyOut>();

        public BindableCollection<IToolBox> Tools { get; } = new BindableCollection<IToolBox>();

        public List<IControlScrapper> ScreenScrappers { get; } = new List<IControlScrapper>();

        ISerializer serializer;

        public ShellViewModel(IEventAggregator eventAggregator, ISerializer serializer, IReadOnlyCollection<IToolBox> tools, IEnumerable<IFlyOut> flyOuts,
            IEnumerable<IControlScrapper> scrappers, IHome homeScreen) : base()
        {
            Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull();
            Guard.Argument(tools, nameof(tools)).NotNull().NotEmpty();
            Guard.Argument(flyOuts, nameof(flyOuts)).NotNull().NotEmpty();
            Guard.Argument(scrappers, nameof(scrappers)).NotNull().NotEmpty();
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;           
         
            DisplayName = "Pixel Automation";        

            this.Tools.AddRange(tools);
            this.ScreenScrappers.AddRange(scrappers);
            this.FlyoutViewModels.AddRange(flyOuts);

            propertyGrid = this.Tools.OfType<PropertyGridViewModel>().FirstOrDefault();
          
            eventAggregator.SubscribeOnUIThread(this);

            _ = ActivateItemAsync(homeScreen as Screen, CancellationToken.None);

           
        }

        public void ToggleFlyout(int index)
        {
            var flyout = this.FlyoutViewModels[index];
            flyout.IsOpen = !flyout.IsOpen;
        }

        public async Task DoNew()
        {
            logger.Debug("DoNew start");
            
            IWindowManager windowManager = IoC.Get<IWindowManager>();
            INewProject newProjectVM = IoC.Get<INewProject>();
            AutomationProject newProject = newProjectVM.NewProject;
            var result = await windowManager.ShowDialogAsync(newProjectVM);
            if (result.HasValue && result.Value)
            {
                await OpenProject(newProject);
            }
            logger.Debug("DoNew end");
        }

        public async Task DoOpen()
        {
            logger.Debug("DoOpen start");

            var fileToOpen = ShowOpenFileDialog();
            logger.Information("Picked project file : {$file} to open", fileToOpen);
            if (string.IsNullOrEmpty(fileToOpen))
                return;
            var automationProject = serializer.Deserialize<AutomationProject>(fileToOpen, null);
            await OpenProject(automationProject);
            logger.Debug("DoOpen end");

        }

        private async Task OpenProject(AutomationProject automationProject)
        {
            logger.Debug("OpenProject start");

            var automationEditor = IoC.Get<IAutomationEditor>();            
            await this.ActivateItemAsync(automationEditor as Screen);
            await automationEditor.DoLoad(automationProject);

            logger.Debug("OpenProject end");

        }

        private string ShowOpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Automation Project (*.atm)|*.atm";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
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

            logger.Debug("Save start");

            if (this.ActiveItem != null)
            {
                if (this.ActiveItem is IEditor editor)
                {
                    editor.DoSave();
                }
            }

            logger.Debug("Save end");

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
            logger.Debug("SaveAll start");

            foreach (var item in this.Items)
            {
                if (item is IEditor editor)
                {
                    editor.DoSave();
                }
            }

            logger.Debug("SaveAll end");
        }

        public bool CanEditDataModel
        {
            get
            {
                return this.ActiveItem != null && this.ActiveItem is IEditor;
            }
        }

        public async Task EditDataModelAsync()
        {
            var activeItem = this.ActiveItem as IEditor;
            if (activeItem == null)
            {
                return;
            }
            await activeItem.EditDataModelAsync();
        }


        public bool CanEditScript
        {
            get
            {
                return this.ActiveItem != null && this.ActiveItem is IAutomationEditor;
            }
        }

        public async Task EditScriptAsync()
        {
            var activeItem = this.ActiveItem as IAutomationEditor;
            if (activeItem == null)
            {
                return;
            }
            await activeItem.EditScriptAsync();
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
                NotifyOfPropertyChange(() => CanEditScript);
                NotifyOfPropertyChange(() => CanSave);
                NotifyOfPropertyChange(() => CanSaveAll);
                NotifyOfPropertyChange(() => CanManage);           
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
            propertyGrid.SetState(message.ObjectToDisplay, message.IsReadOnly, message.SaveCommand, message.CanSaveCommand);          
            await Task.CompletedTask;
        }

        #endregion property grid

        public async Task HandleAsync(ActivateScreenNotification activateScreenNotification, CancellationToken cancellationToken)
        {
            if (activateScreenNotification?.ScreenToActivate != null)
            {            
                await this.ActivateItemAsync(activateScreenNotification.ScreenToActivate, CancellationToken.None);
            }
            await Task.CompletedTask;
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken)
        {
            ITestExplorer testExplorer = this.Tools.OfType<ITestExplorer>().FirstOrDefault();
            var result = MessageBox.Show("Are you sure you want to exit? Any unsaved changes will be lost.",
               "Confirm Quit", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
            {
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        
    }
}
