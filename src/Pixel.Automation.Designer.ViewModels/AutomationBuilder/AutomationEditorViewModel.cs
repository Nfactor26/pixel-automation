using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.ViewModels;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Windows;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;
using MessageBox = System.Windows.MessageBox;

namespace Pixel.Automation.Designer.ViewModels
{

    public class AutomationEditorViewModel : EditorViewModel, IAutomationEditor
    {
        #region data members        

        private readonly ILogger logger = Log.ForContext<AutomationEditorViewModel>();
        private readonly IAutomationProjectManager projectManager;
        private readonly IComponentViewBuilder componentViewBuilder;
        private readonly IServiceResolver serviceResolver;
        private readonly ITestExplorer testExplorer;
        private readonly ITestExplorerHost testExplorerHost;        
        private readonly ITestDataRepository testDataRepository;
        private readonly ITestDataRepositoryHost testDataRepositoryHost;       
        #endregion data members

        #region constructor
        public AutomationEditorViewModel(IServiceResolver serviceResolver, IEventAggregator globalEventAggregator, IWindowManager windowManager, 
            INotificationManager notificationManager, ISerializer serializer, IEntityManager entityManager, ITestExplorer testExplorer, ITestDataRepository testDataRepository,
            IAutomationProjectManager projectManager, IComponentViewBuilder componentViewBuilder, IScriptExtactor scriptExtractor, IReadOnlyCollection<IAnchorable> anchorables, 
            IVersionManagerFactory versionManagerFactory, IDropTarget dropTarget, ApplicationSettings applicationSettings)
            : base(globalEventAggregator, windowManager, notificationManager, serializer, entityManager, projectManager, scriptExtractor, versionManagerFactory, dropTarget, applicationSettings)
        {

            this.serviceResolver = Guard.Argument(serviceResolver, nameof(serviceResolver)).NotNull().Value;
            this.projectManager = Guard.Argument(projectManager, nameof(projectManager)).NotNull().Value;
            this.componentViewBuilder = Guard.Argument(componentViewBuilder, nameof(componentViewBuilder)).NotNull().Value;
            this.testExplorer = Guard.Argument(testExplorer, nameof(testExplorer)).NotNull().Value;
            this.testDataRepository = Guard.Argument(testDataRepository, nameof(testDataRepository)).NotNull().Value;           
            Guard.Argument(anchorables, nameof(anchorables)).NotNull().NotEmpty().DoesNotContainNull();

            foreach (var item in anchorables)
            {               
                if(item is ITestDataRepositoryHost repositoryHost)
                {
                    testDataRepositoryHost = repositoryHost;
                }

                if(item is ITestExplorerHost explorerHost)
                {
                    testExplorerHost = explorerHost;                  
                }
            }         
        }

        #endregion constructor                    

        #region Automation Project 


        public AutomationProject CurrentProject { get; private set; }

        public async Task DoLoad(AutomationProject project, VersionInfo versionToLoad = null)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DoLoad), ActivityKind.Internal))
            {
                Guard.Argument(project, nameof(project)).NotNull();

                this.CurrentProject = project;
                this.DisplayName = project.Name;

                //Always open the most recent non-deployed version if no version is specified
                var targetVersion = versionToLoad ?? project.LatestActiveVersion;
                if (targetVersion != null)
                {
                    activity?.SetTag("AutomationProject", project.Name);
                    activity?.SetTag("ProjectVersion", versionToLoad.ToString());
                    logger.Information("Version : '{0}' of project : '{1}' will be loaded.", targetVersion, project.Name);

                    await this.projectManager.Load(project, targetVersion);
                    UpdateWorkFlowRoot();
                    return;
                }
                activity?.SetStatus(ActivityStatusCode.Error, "No active version could be located");
                throw new InvalidDataException($"No active version could be located for project : {project.Name}");
            }
        } 

        /// <summary>
        /// Add, remove, edit data models to be used across the project and test cases.
        /// </summary>
        /// <returns></returns>
        public override async Task EditDataModelAsync()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(EditDataModelAsync), ActivityKind.Internal))
            {
                try
                {
                    logger.Information($"Opening code editor for editing data model for project : {this.CurrentProject.Name}");
                    await this.projectManager.DownloadDataModelFilesAsync();
                    var editorFactory = this.EntityManager.GetServiceOfType<ICodeEditorFactory>();
                    var projectFileSystem = this.projectManager.GetProjectFileSystem();
                    using (var editor = editorFactory.CreateMultiCodeEditorScreen())
                    {
                        foreach (var file in Directory.GetFiles(projectFileSystem.DataModelDirectory, "*.cs"))
                        {
                            await editor.AddDocumentAsync(Path.GetFileName(file), this.CurrentProject.Name, File.ReadAllText(file), false);
                        }

                        await editor.AddDocumentAsync($"{Constants.AutomationProcessDataModelName}.cs", this.CurrentProject.Name, string.Empty, false);
                        await editor.OpenDocumentAsync($"{Constants.AutomationProcessDataModelName}.cs", this.CurrentProject.Name);

                        bool? hasChanges = await this.windowManager.ShowDialogAsync(editor);
                        var editorDocumentStates = editor.GetCurrentEditorState();

                        if (hasChanges.HasValue && !hasChanges.Value)
                        {
                            logger.Information("Discarding changes for data model files");
                            foreach (var document in editorDocumentStates)
                            {
                                if (document.IsNewDocument)
                                {
                                    File.Delete(Path.Combine(projectFileSystem.DataModelDirectory, document.TargetDocument));
                                    logger.Information("Delete file {@0} from data model files", document);
                                }
                            }
                            return;
                        }

                        foreach (var document in editorDocumentStates)
                        {
                            if ((document.IsNewDocument || document.IsModified) && !document.IsDeleted)
                            {
                                await this.projectManager.AddOrUpdateDataFileAsync(Path.Combine(projectFileSystem.DataModelDirectory, document.TargetDocument));
                            }
                            if (document.IsDeleted && !document.IsNewDocument)
                            {
                                await this.projectManager.DeleteDataFileAsync(Path.Combine(projectFileSystem.DataModelDirectory, document.TargetDocument));
                            }
                            logger.Information("Updated state of data model file {@0}", document);
                        }
                    }
                    await this.Reload();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to edit data model for project : '{0}'", this.CurrentProject.Name);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }           
        }

        /// <summary>
        /// Edit the Initialization script file for the automation process.
        /// Initialization script can be over-ridden in test runner to initialize automation process
        /// environment differently e.g. specify a different browser to be launched then what is 
        /// configured as the PreferredBrowser on WebApplication.
        /// </summary>
        /// <returns></returns>
        public override async Task EditScriptAsync()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(EditScriptAsync), ActivityKind.Internal))
            {
                try
                {
                    await this.projectManager.DownloadFileByNameAsync(Constants.InitializeEnvironmentScript);
                    var entityManager = this.EntityManager;
                    var fileSystem = entityManager.GetCurrentFileSystem();
                    var scriptFile = Path.Combine(fileSystem.ScriptsDirectory, Constants.InitializeEnvironmentScript);

                    //Add the project to workspace
                    var scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>();
                    scriptEditorFactory.AddProject(this.CurrentProject.Name, new string[] { }, this.EntityManager.Arguments.GetType());
                    scriptEditorFactory.AddDocument(fileSystem.GetRelativePath(scriptFile), this.CurrentProject.Name, File.ReadAllText(scriptFile));
                    //Create script editor and open the document to edit
                    using (IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditorScreen())
                    {
                        scriptEditorScreen.OpenDocument(fileSystem.GetRelativePath(scriptFile), this.CurrentProject.Name, string.Empty);
                        var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);
                        if (result.HasValue && result.Value)
                        {
                            var scriptEngine = entityManager.GetScriptEngine();
                            scriptEngine.ClearState();
                            await scriptEngine.ExecuteFileAsync(scriptFile);
                            await this.projectManager.AddOrUpdateDataFileAsync(scriptFile);
                            logger.Information("Updated script file : {0}", scriptFile);
                        }
                        scriptEditorFactory.RemoveProject(this.CurrentProject.Name);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to edit initialization script for project : '{0}'", this.CurrentProject.Name);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }            
        }

        protected override void BuildWorkFlow(EntityComponentViewModel root)
        {
            base.BuildWorkFlow(root);
            this.componentViewBuilder.SetRoot(root);
        }

        protected override async Task Reload()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(Reload), ActivityKind.Internal))
            {
                logger.Information("Closing all open TestCases for project: {0}", this.CurrentProject.Name);

                //closing fixture will also close test cases
                var testCaseEntities = this.EntityManager.RootEntity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants);
                var testFixtures = testCaseEntities.Select(t => t.Parent as TestFixtureEntity).Distinct();
                foreach (var fixture in testFixtures)
                {
                    await this.testExplorer.CloseTestFixtureAsync(fixture.Tag);
                }

                await this.projectManager.Reload();

                UpdateWorkFlowRoot();

                //Opening test case will also open parent TestFixture if not already open.
                logger.Information("Opening all test cases back for projoect : {0}", this.CurrentProject.Name);
                foreach (var testEntity in testCaseEntities)
                {
                    await this.testExplorer.OpenTestCaseAsync(testEntity.Tag);
                }
            }            
        }


        #endregion Automation Project

        #region OnLoad

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            //whenever we activate the automation editor, activate the test explorer and test data repository associated with it as well
            await this.testExplorerHost.ActivateItemAsync(this.testExplorer);
            await this.testDataRepositoryHost.ActivateItemAsync(this.testDataRepository);
            await base.OnActivateAsync(cancellationToken);
            logger.Debug("Automation Project screen - {0} was activated", this.CurrentProject.Name);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            //whenever we deactivate the automation editor, we deactivate the test explorer and test data repository associated with it as well.
            //false parameter tells that we should not close the test explorer but only deactivate.
            await this.testExplorerHost.DeactivateItemAsync(this.testExplorer, false);
            await this.testDataRepositoryHost.DeactivateItemAsync(this.testExplorer, false);          
            await base.OnDeactivateAsync(close, cancellationToken);
            logger.Debug("Automation Project screen - {0} was de-activated", this.CurrentProject.Name);
        }

        #endregion OnLoad

        #region Save project

        public override async Task DoSave()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DoSave), ActivityKind.Internal))
            {
                try
                {
                    await this.testExplorer.SaveAll();
                    await projectManager.Save();
                    await notificationManager.ShowSuccessNotificationAsync("Project was saved.");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to save project : '{0}'", this.CurrentProject.Name);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }               
        }
      
        /// <inheritdoc/>  
        public async Task ManagePrefabReferencesAsync()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(ManagePrefabReferencesAsync), ActivityKind.Internal))
            {
                try
                {
                    var projectFileSystem = this.projectManager.GetProjectFileSystem() as IProjectFileSystem;
                    var versionManager = this.versionManagerFactory.CreatePrefabReferenceManager(this.projectManager.GetReferenceManager());
                    await this.windowManager.ShowDialogAsync(versionManager);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error on manage prefab reference screen");
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }          
        }      

        #endregion Save project

        #region Close Screen

        public override async void CloseScreen()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close? Any unsaved changes will be lost.", "Confirm Close", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CloseScreen), ActivityKind.Internal))
                {
                    try
                    {
                        await CloseAsync();
                        logger.Debug("Automation Project screen - {0} was closed", this.CurrentProject.Name);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "There was an error while trying to close project : '{0}'", this.CurrentProject.Name);
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await notificationManager.ShowErrorNotificationAsync(ex);
                    }
                }               
            }        
        }

        protected override async Task CloseAsync()
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
            await this.testDataRepositoryHost.DeactivateItemAsync(this.testDataRepository, true);              
            await this.globalEventAggregator.PublishOnUIThreadAsync(new EditorClosedNotification<AutomationProject>(this.CurrentProject));
            await this.TryCloseAsync(true);
           
            this.Dispose();
        }


        #endregion Close Screen
       
        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            this.serviceResolver.Dispose();
        }

    }
}
