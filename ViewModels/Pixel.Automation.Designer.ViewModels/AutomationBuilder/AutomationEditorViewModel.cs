using Caliburn.Micro;
using Dawn;
using GongSolutions.Wpf.DragDrop;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
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
        private readonly ITestExplorer testExplorerToolBox;
        private readonly ITestRepositoryManager testRepositoryManager;
        private readonly IServiceResolver serviceResolver;        

        private readonly TestDataRepositoryViewModel testDataRepositoryViewModel;          
        TestDataRepository testDataRepository = default;

        #endregion data members

        #region constructor
        public AutomationEditorViewModel(IServiceResolver serviceResolver, IEventAggregator globalEventAggregator, IWindowManager windowManager,  ISerializer serializer, IEntityManager entityManager,
            IAutomationProjectManager projectManager, ITestRepositoryManager testRepositoryManager,
            IScriptExtactor scriptExtractor, IReadOnlyCollection<IToolBox> tools, IVersionManagerFactory versionManagerFactory, IDropTarget dropTarget, ApplicationSettings applicationSettings) : base(
            globalEventAggregator, windowManager, serializer, entityManager, scriptExtractor, tools, versionManagerFactory, dropTarget, applicationSettings)
        {

            this.serviceResolver = Guard.Argument(serviceResolver, nameof(serviceResolver)).NotNull().Value;
            this.projectManager = Guard.Argument(projectManager, nameof(projectManager)).NotNull().Value;             
            this.testRepositoryManager = Guard.Argument(testRepositoryManager, nameof(testRepositoryManager)).NotNull().Value; 

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

        #region Manage components

        public override void DeleteComponent(IComponent component)
        {
            //when deleting a PrefabEntity, we need to remove it from PrefabReferences mapping file
            if (component is PrefabEntity prefabEntity)
            {
                component.TryGetAnsecstorOfType<TestCaseEntity>(out TestCaseEntity testCaseEntity);
                base.DeleteComponent(component);
                var fileSystem = projectManager.GetProjectFileSystem() as IProjectFileSystem;
                var prefabReferences = fileSystem.LoadFile<PrefabReferences>(fileSystem.PrefabReferencesFile);
                var prefabReference = prefabReferences.GetPrefabReference(prefabEntity.PrefabId);
                prefabReference.RemoveReference(new ReferringEntityDetails() { EntityId = prefabEntity.Id, TestCaseId = testCaseEntity?.Id });
                fileSystem.SaveToFile<PrefabReferences>(prefabReferences, fileSystem.WorkingDirectory, Path.GetFileName(fileSystem.PrefabReferencesFile));
                return;
            }
            base.DeleteComponent(component);
        }

        #endregion Manage components

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


        public override async Task EditDataModel()
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

                    await editor.AddDocumentAsync("DataModel.cs", this.CurrentProject.Name, string.Empty, false);
                    await editor.OpenDocumentAsync("DataModel.cs", this.CurrentProject.Name);                 
              
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
                    await this.testRepositoryManager.CloseTestFixtureAsync(fixture.Tag);
                }

                await this.projectManager.Refresh();

                UpdateWorkFlowRoot();

                //Opening test case will also open parent TestFixture if not already open.
                logger.Information($"Opening all test cases back for projoect : {this.CurrentProject.Name}");
                foreach (var testEntity in testCaseEntities)
                {
                    await this.testRepositoryManager.OpenTestCaseAsync(testEntity.Tag);
                }
            }
            catch (Exception ex)
            { 
                logger.Information(ex, ex.Message);
            }
        }       

        private void InitializeTestProcess()
        {         
            this.testRepositoryManager.Initialize();
            this.testExplorerToolBox?.SetActiveInstance(this.testRepositoryManager);

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
            try
            {
                this.testRepositoryManager.SaveAll();
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
            await this.windowManager.ShowDialogAsync(versionManager);

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
                await this.testRepositoryManager.CloseTestCaseAsync(testEntity.Tag);
            }

            this.testDataRepositoryViewModel?.ClearActiveInstance();
            this.testDataRepository = null;
            
            this.Dispose();           
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
