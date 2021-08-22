using Caliburn.Micro;
using Dawn;
using GongSolutions.Wpf.DragDrop;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.Notfications;
using Pixel.Automation.TestData.Repository.ViewModels;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Pixel.Automation.Designer.ViewModels
{

    public class AutomationEditorViewModel :  EditorViewModel , IAutomationEditor
    {
        #region data members        

        private readonly ILogger logger = Log.ForContext<AutomationEditorViewModel>();
        private readonly IAutomationProjectManager projectManager;      
        private readonly IServiceResolver serviceResolver;
        private readonly ITestExplorer testExplorer;
        private readonly ITestExplorerHost testExplorerHost;

        private readonly TestDataRepositoryViewModel testDataRepositoryViewModel;          
        TestDataRepository testDataRepository = default;

        #endregion data members

        #region constructor
        public AutomationEditorViewModel(IServiceResolver serviceResolver, IEventAggregator globalEventAggregator, IWindowManager windowManager,  ISerializer serializer, IEntityManager entityManager, ITestExplorer testExplorer,
            IAutomationProjectManager projectManager, IScriptExtactor scriptExtractor, IReadOnlyCollection<IToolBox> tools, IVersionManagerFactory versionManagerFactory, IDropTarget dropTarget, ApplicationSettings applicationSettings) : base(
            globalEventAggregator, windowManager, serializer, entityManager, scriptExtractor, tools, versionManagerFactory, dropTarget, applicationSettings)
        {

            this.serviceResolver = Guard.Argument(serviceResolver).NotNull().Value;
            this.projectManager = Guard.Argument(projectManager).NotNull().Value;           
            this.testExplorer = Guard.Argument(testExplorer).NotNull().Value; 

            foreach (var item in Tools)
            {               
                if(item is TestDataRepositoryViewModel)
                {
                    testDataRepositoryViewModel = item as TestDataRepositoryViewModel;
                }

                if(item is ITestExplorerHost host)
                {
                    testExplorerHost = host;                  
                }
            }

         
        }

        #endregion constructor                    

        #region Automation Project 


        public AutomationProject CurrentProject { get; private set; }

        public async Task DoLoad(AutomationProject project, VersionInfo versionToLoad = null)
        {
            Guard.Argument(project, nameof(project)).NotNull();    
      
            this.CurrentProject = project;
            this.DisplayName = project.Name;

            //Always open the most recent non-deployed version if no version is specified
            var targetVersion = versionToLoad ?? project.ActiveVersion;
            logger.Information($"Version : {targetVersion} will be loaded.");
            if(targetVersion != null)
            {
                await this.projectManager.Load(project, targetVersion);            
                UpdateWorkFlowRoot();             
                return;
            }

            throw new InvalidDataException($"No active version could be located for project : {project.Name}");
          
        } 

        /// <summary>
        /// Add, remove, edit data models to be used across the project and test cases.
        /// </summary>
        /// <returns></returns>
        public override async Task EditDataModelAsync()
        {
            try
            {
                logger.Information($"Opening code editor for editing data model for project : {this.CurrentProject.Name}");
                var editorFactory = this.EntityManager.GetServiceOfType<ICodeEditorFactory>();
                using (var editor = editorFactory.CreateMultiCodeEditorScreen())
                {
                    foreach (var file in Directory.GetFiles(this.projectManager.GetProjectFileSystem().DataModelDirectory, "*.cs"))
                    {
                        await editor.AddDocumentAsync(Path.GetFileName(file), this.CurrentProject.Name, File.ReadAllText(file), false);
                    }

                    await editor.AddDocumentAsync($"{Constants.ProcessDataModelName}.cs", this.CurrentProject.Name, string.Empty, false);
                    await editor.OpenDocumentAsync($"{Constants.ProcessDataModelName}.cs", this.CurrentProject.Name);                 
              
                    bool? hasChanges = await this.windowManager.ShowDialogAsync(editor);
                    if (hasChanges.HasValue && !hasChanges.Value)
                    {
                        return;
                    }
                }
                logger.Information($"Editing data model completed for project : {this.CurrentProject.Name}");

                logger.Information($"Closing all open TestCases for project: {this.CurrentProject.Name}");

                //closing fixture will also close test cases
                var testCaseEntities = this.EntityManager.RootEntity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants);
                var testFixtures = testCaseEntities.Select(t => t.Parent as TestFixtureEntity).Distinct();
                foreach (var fixture in testFixtures)
                {
                    await this.testExplorer.CloseTestFixtureAsync(fixture.Tag);
                }

                await this.projectManager.Refresh();

                UpdateWorkFlowRoot();

                //Opening test case will also open parent TestFixture if not already open.
                logger.Information($"Opening all test cases back for projoect : {this.CurrentProject.Name}");
                foreach (var testEntity in testCaseEntities)
                {
                    await this.testExplorer.OpenTestCaseAsync(testEntity.Tag);
                }
            }
            catch (Exception ex)
            { 
                logger.Information(ex, ex.Message);
            }
        }

        /// <summary>
        /// Edit the Initialization script file for the automation process.
        /// Initialization script can be over-ridden in test runner to initialize automation process
        /// environment differently e.g. specify a different browser to be launched then what is 
        /// configured as the PreferredBrowser on WebApplication.
        /// </summary>
        /// <returns></returns>
        public async Task EditScriptAsync()
        {
            try
            {
                var entityManager = this.EntityManager;
                var fileSystem = entityManager.GetCurrentFileSystem();
                var scriptFile = Path.Combine(fileSystem.ScriptsDirectory, Constants.InitializeEnvironmentScript);             
              
                //Add the project to workspace
                var scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                scriptEditorFactory.AddProject(this.CurrentProject.GetProjectName(), new string[] {}, this.EntityManager.Arguments.GetType());
                scriptEditorFactory.AddDocument(fileSystem.GetRelativePath(scriptFile), this.CurrentProject.GetProjectName(), File.ReadAllText(scriptFile));
                //Create script editor and open the document to edit
                IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditor();
                scriptEditorScreen.OpenDocument(fileSystem.GetRelativePath(scriptFile), this.CurrentProject.GetProjectName(), string.Empty);
                var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);
                if (result.HasValue && result.Value)
                {
                    var scriptEngine = entityManager.GetScriptEngine();
                    scriptEngine.ClearState();
                    await scriptEngine.ExecuteFileAsync(scriptFile);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        private void InitializeTestProcess()
        {        
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
            InitializeTestProcess();
            //whenever we activate the automation editor, activate the test explorer associated to it as well
            await this.testExplorerHost.ActivateItemAsync(this.testExplorer);
            await base.OnActivateAsync(cancellationToken);
            logger.Information($"Automation Project : {this.CurrentProject.Name} was activated");
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            //whenever we deactivate the automation editor, we deactivate the test explorer associated to it as well.
            //false parameter tells that we should not close the test explorer but only deactivate.
            await this.testExplorerHost.DeactivateItemAsync(this.testExplorer, false);
            this.testDataRepositoryViewModel?.ClearActiveInstance();
            await base.OnDeactivateAsync(close, cancellationToken);
            logger.Information($"Automation Project : {this.CurrentProject.Name} was deactivated");
        }

        #endregion OnLoad

        #region Save project

        public override async Task DoSave()
        {
            try
            {
                this.testExplorer.SaveAll();
                await projectManager.Save();
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        public async override Task Manage()
        {
            await DoSave();
            var versionManager = this.versionManagerFactory.CreateProjectVersionManager(this.CurrentProject);        
            var result = await this.windowManager.ShowDialogAsync(versionManager);

            if (result.HasValue && result.Value)
            {
                var fileSystem = this.projectManager.GetProjectFileSystem() as IVersionedFileSystem;
                fileSystem.SwitchToVersion(this.CurrentProject.ActiveVersion);

                //This will update Code editor and script editor to point to the new workspace directory
                var codeEditorFactory = this.EntityManager.GetServiceOfType<ICodeEditorFactory>();
                codeEditorFactory.SwitchWorkingDirectory(fileSystem.DataModelDirectory);
                var scriptEditorFactory = this.EntityManager.GetServiceOfType<IScriptEditorFactory>();
                scriptEditorFactory.SwitchWorkingDirectory(fileSystem.WorkingDirectory);
             
            }        
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
            await SetSelectedItem(null);
            this.globalEventAggregator.Unsubscribe(this);

            //when test cases are closed by test explorer, EntityManageres created for each open tests are also disposed.        
            var testCaseEntities = this.EntityManager.RootEntity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants);
            foreach (var testEntity in testCaseEntities)
            {
                await this.testExplorer.CloseTestCaseAsync(testEntity.Tag);
            }
            await this.testExplorerHost.DeactivateItemAsync(this.testExplorer, true);

            this.testDataRepositoryViewModel?.ClearActiveInstance();
            this.testDataRepository = null;
            
            this.Dispose();
            await this.globalEventAggregator.PublishOnBackgroundThreadAsync(new EditorClosedNotification(this.CurrentProject));
            await this.TryCloseAsync(true);            
        }


        #endregion Close Screen

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            this.serviceResolver.Dispose();
        }
    }
}
