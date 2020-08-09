using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Designer.ViewModels.VersionManager;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.TestData.Repository.ViewModels;
using Pixel.Automation.TestExplorer;
using Pixel.Persistence.Services.Client;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Pixel.Automation.Designer.ViewModels
{

    public class AutomationEditorViewModel :  EditorViewModel , IAutomationEditor
    {
        #region data members        

        private readonly ILogger logger = Log.ForContext<AutomationEditorViewModel>();
        private readonly ITestExplorer testExplorerToolBox;
        private readonly TestDataRepositoryViewModel testDataRepositoryViewModel;     
 
        TestRepositoryManager testCaseManager = default;
        TestDataRepository testDataRepository = default;

        #endregion data members

        #region constructor
        public AutomationEditorViewModel(IEventAggregator globalEventAggregator, IServiceResolver serviceResolver, ISerializer serializer,
            IScriptExtactor scriptExtractor, IToolBox[] toolBoxes) : base(
            globalEventAggregator, serviceResolver, serializer, scriptExtractor, toolBoxes)
        {
        
            foreach (var item in Tools)
            {               
                if(item is ITestExplorer)
                {
                    testExplorerToolBox = item as ITestExplorer;
                    continue;
                }

                if(item is TestDataRepositoryViewModel)
                {
                    testDataRepositoryViewModel = item as TestDataRepositoryViewModel;
                }
            }             
        }
      
        #endregion constructor               

        #region Automation Project
      
        AutomationProjectManager projectManager;

        public AutomationProject CurrentProject { get; private set; }

        public async Task DoLoad(AutomationProject project, VersionInfo versionToLoad = null)
        {
            Guard.Argument(project, nameof(project)).NotNull();

            //Getting AutomationProjectManager from EntityManager service resolver instead of injecting this as a constructor parameter since we want
            //all dependencies of automation project manager to be resolved by entity manager's service resolver ( which uses child kernel to resolve this)
            this.projectManager = this.EntityManager.GetServiceOfType<AutomationProjectManager>().WithEntityManager(this.EntityManager) as AutomationProjectManager;

            this.CurrentProject = project;
            this.DisplayName = project.Name;

            //Always open the most recent non-deployed version if no version is specified
            var targetVersion = versionToLoad ?? project.ActiveVersion;
            if(targetVersion != null)
            {
                await this.projectManager.Load(project, targetVersion);
                UpdateWorkFlowRoot();
                return;
            }

            throw new InvalidDataException($"No active version could be located for project : {project.Name}");
          
        } 


        public override async Task EditDataModel()
        {         
            var editorFactory = this.EntityManager.GetServiceOfType<ICodeEditorFactory>();
            using (var editor = editorFactory.CreateMultiCodeEditorScreen())
            {
                foreach (var file in Directory.GetFiles(this.projectManager.GetProjectFileSystem().DataModelDirectory, "*.cs"))
                {
                    await editor.AddDocumentAsync(Path.GetFileName(file), File.ReadAllText(file), false);
                }

                await editor.AddDocumentAsync("DataModel.cs", string.Empty, false);
                await editor.OpenDocumentAsync("DataModel.cs");

                IWindowManager windowManager = this.EntityManager.GetServiceOfType<IWindowManager>();
                await windowManager.ShowDialogAsync(editor);              
            }

            var testCaseEntities = this.EntityManager.RootEntity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants);         
            foreach (var testEntity in testCaseEntities)
            {
                this.testCaseManager.DoneEditing(testEntity.Tag);
            }
           
            this.projectManager.Refresh();
           
            foreach (var testEntity in testCaseEntities)
            {
                this.testCaseManager.OpenForEdit(testEntity.Tag);
            }

            UpdateWorkFlowRoot();

            logger.Information($"Data model was edited for automation project : {this.CurrentProject.Name}");
        }       

        private void InitializeTestProcess()
        {
            if(this.testCaseManager == null)
            {
                ITestRunner testRunner = this.EntityManager.GetServiceOfType<ITestRunner>();
                IEventAggregator eventAggregator = this.EntityManager.GetServiceOfType<IEventAggregator>();                          
                IWindowManager windowManager = this.EntityManager.GetServiceOfType<IWindowManager>();

                this.testCaseManager = new TestRepositoryManager(eventAggregator,this.projectManager, this.projectManager.GetProjectFileSystem() as IProjectFileSystem, testRunner, windowManager);
            }
            this.testExplorerToolBox?.SetActiveInstance(this.testCaseManager);

            if(this.testDataRepository == null)
            {
                this.testDataRepository = this.EntityManager.GetServiceOfType<TestDataRepository>();            
            }          
            this.testDataRepositoryViewModel.SetActiveInstance(this.testDataRepository);
        }

        #endregion Automation Project

        #region OnLoad

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        { 
            switch (this.CurrentProject.ProjectType)
            {
                case ProjectType.TestAutomation:                 
                    InitializeTestProcess();
                    break;
                default:
                    break;
            }                  
            await base.OnActivateAsync(cancellationToken);
            logger.Information($"Automation Project : {this.CurrentProject.Name} was activated");
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            this.testExplorerToolBox?.ClearActiveInstance();
            this.testDataRepositoryViewModel?.ClearActiveInstance();
            await base.OnDeactivateAsync(close, cancellationToken);
            logger.Information($"Automation Project : {this.CurrentProject.Name} was deactivated");
        }

        #endregion OnLoad

        #region Save project

        public override async Task DoSave()
        {
           await projectManager.Save();
        }

        public async override Task Manage()
        {
            await DoSave();

            var workspaceManagerFactory = this.EntityManager.GetServiceOfType<IWorkspaceManagerFactory>();
            var applicationDataManager = this.EntityManager.GetServiceOfType<IApplicationDataManager>();
            ProjectVersionManagerViewModel versionManager = new ProjectVersionManagerViewModel(this.CurrentProject, workspaceManagerFactory, applicationDataManager, this.serializer);
            IWindowManager windowManager = this.EntityManager.GetServiceOfType<IWindowManager>();
            await windowManager.ShowDialogAsync(versionManager);

            var fileSystem = this.projectManager.GetProjectFileSystem() as IVersionedFileSystem;
            fileSystem.SwitchToVersion(this.CurrentProject.ActiveVersion);
        }      

        #endregion Save project

        #region Close Screen

        public override async void CloseScreen()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close? Any unsaved changes will be lost.", "Confirm Close", MessageBoxButton.OKCancel);
            try
            {
                if (result == MessageBoxResult.OK)
                {
                    await CloseAsync();
                    logger.Information($"{this.CurrentProject.Name} was closed");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        protected override async  Task CloseAsync()
        {
            SetSelectedItem(null);
            this.globalEventAggregator.Unsubscribe(this);

            //when test cases are closed by test explorer, EntityManageres created for each open tests are also disposed.
            this.testExplorerToolBox?.ClearActiveInstance();
            var testCaseEntities = this.EntityManager.RootEntity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants);
            foreach (var testEntity in testCaseEntities)
            {
                this.testCaseManager.DoneEditing(testEntity.Tag);
            }

            this.testDataRepositoryViewModel?.ClearActiveInstance();
            this.testDataRepository = null;
            this.testCaseManager = null;
            this.projectManager = null;                                                

            this.Dispose();           
            await this.TryCloseAsync(true);            
        }

        #endregion Close Screen

      
    }
}
