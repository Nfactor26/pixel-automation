using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System.Diagnostics;
using System.IO;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;

namespace Pixel.Automation.Designer.ViewModels
{
    public class PrefabEditorViewModel : EditorViewModel , IPrefabEditor
    {
        private readonly ILogger logger = Log.ForContext<PrefabEditorViewModel>();
        private readonly IServiceResolver serviceResolver;
        private readonly IPrefabProjectManager projectManager;  
          
        #region constructor

        public PrefabEditorViewModel(IServiceResolver serviceResolver, IEventAggregator globalEventAggregator, IWindowManager windowManager,
            INotificationManager notificationManager, ISerializer serializer, IEntityManager entityManager, IPrefabProjectManager projectManager, IScriptExtactor scriptExtractor, 
            IVersionManagerFactory versionManagerFactory, IDropTarget dropTarget, ApplicationSettings applicationSettings):
            base(globalEventAggregator, windowManager, notificationManager, serializer, entityManager, projectManager, scriptExtractor, versionManagerFactory, dropTarget, applicationSettings)
        {
            this.serviceResolver = Guard.Argument(serviceResolver, nameof(serviceResolver)).NotNull().Value;
            this.projectManager = Guard.Argument(projectManager, nameof(projectManager)).NotNull().Value;
        }

        #endregion constructor

        #region Automation Project
       
        public PrefabProject PrefabProject { get; private set; }       

        public async Task DoLoad(PrefabProject prefabProject, VersionInfo versionToLoad = null)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DoLoad), ActivityKind.Internal))
            {
                Guard.Argument(prefabProject, nameof(prefabProject)).NotNull();

                this.PrefabProject = prefabProject;
                this.DisplayName = prefabProject.Name;

                var targetVersion = versionToLoad ?? PrefabProject.LatestActiveVersion;
                if (targetVersion != null)
                {
                    activity?.SetTag("PrefabProject", prefabProject.Name);
                    activity?.SetTag("ProjectVersion", versionToLoad.ToString());
                    logger.Information("Version : '{0}' of prefab project : '{1}' will be loaded.", targetVersion, prefabProject.Name);

                    await this.projectManager.Load(prefabProject, targetVersion);
                    UpdateWorkFlowRoot();
                    return;
                }
                activity?.SetStatus(ActivityStatusCode.Error, "No active version could be located");
                throw new InvalidDataException($"No active version could be located for prefab project : {this.PrefabProject.Name}");
            }             
         
        }

        /// <summary>
        /// Edit the PrefabDataModel that was generated while creating the Prefab.
        /// </summary>
        /// <returns></returns>
        public override async Task EditDataModelAsync()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(EditDataModelAsync), ActivityKind.Internal))
            {
                try
                {
                    await this.projectManager.DownloadDataModelFilesAsync();
                    var editorFactory = this.EntityManager.GetServiceOfType<ICodeEditorFactory>();
                    var prefabFileSystem = this.projectManager.GetProjectFileSystem();
                    using (var editor = editorFactory.CreateMultiCodeEditorScreen())
                    {
                        foreach (var file in Directory.GetFiles(this.projectManager.GetProjectFileSystem().DataModelDirectory, "*.cs"))
                        {
                            await editor.AddDocumentAsync(Path.GetFileName(file), this.PrefabProject.Name, File.ReadAllText(file), false);
                        }
                        await editor.AddDocumentAsync($"{Constants.PrefabDataModelName}.cs", this.PrefabProject.Name, string.Empty, false);
                        await editor.OpenDocumentAsync($"{Constants.PrefabDataModelName}.cs", this.PrefabProject.Name);

                        bool? hasChanges = await this.windowManager.ShowDialogAsync(editor);
                        var editorDocumentStates = editor.GetCurrentEditorState();

                        if (hasChanges.HasValue && !hasChanges.Value)
                        {
                            logger.Information("Discarding changes for data model files");
                            foreach (var document in editorDocumentStates)
                            {
                                if (document.IsNewDocument)
                                {
                                    File.Delete(Path.Combine(prefabFileSystem.DataModelDirectory, document.TargetDocument));
                                    logger.Information("Delete file {@0} from data model files", document);
                                }
                            }
                            return;
                        }

                        foreach (var document in editorDocumentStates)
                        {
                            if ((document.IsNewDocument || document.IsModified) && !document.IsDeleted)
                            {
                                await this.projectManager.AddOrUpdateDataFileAsync(Path.Combine(prefabFileSystem.DataModelDirectory, document.TargetDocument));
                            }
                            if (document.IsDeleted && !document.IsNewDocument)
                            {
                                await this.projectManager.DeleteDataFileAsync(Path.Combine(prefabFileSystem.DataModelDirectory, document.TargetDocument));
                            }
                            logger.Information("Updated state of data model file {@0}", document);
                        }
                    }
                    var editorReferences = this.projectManager.GetReferenceManager().GetEditorReferences();
                    await this.Reload(editorReferences, editorReferences);
                }
                catch (Exception ex)
                {
                    logger.Information(ex, "There was an error while trying to edit data model for prefab : '{0}'", this.PrefabProject.Name);
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
                    scriptEditorFactory.AddProject(this.PrefabProject.Name, new string[] { }, this.EntityManager.Arguments.GetType());
                    scriptEditorFactory.AddDocument(fileSystem.GetRelativePath(scriptFile), this.PrefabProject.Name, File.ReadAllText(scriptFile));
                    //Create script editor and open the document to edit
                    using (IScriptEditorScreen scriptEditorScreen = scriptEditorFactory.CreateScriptEditorScreen())
                    {
                        scriptEditorScreen.OpenDocument(fileSystem.GetRelativePath(scriptFile), this.PrefabProject.Name, string.Empty);
                        var result = await this.windowManager.ShowDialogAsync(scriptEditorScreen);
                        if (result.HasValue && result.Value)
                        {
                            var scriptEngine = entityManager.GetScriptEngine();
                            scriptEngine.ClearState();
                            await scriptEngine.ExecuteFileAsync(scriptFile);
                            await this.projectManager.AddOrUpdateDataFileAsync(scriptFile);
                            logger.Information("Updated script file : {0}", scriptFile);
                        }
                        scriptEditorFactory.RemoveProject(this.PrefabProject.Name);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to edit initialization script for project : '{0}'", this.PrefabProject.Name);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
        }

        protected override async Task Reload(EditorReferences existing, EditorReferences updated)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(Reload), ActivityKind.Internal))
            {              
                await this.projectManager.Reload(existing, updated);
                this.UpdateWorkFlowRoot();
            }           
        }

        #endregion Automation Project     

        #region Save project

        public override async Task DoSave()
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DoSave), ActivityKind.Internal))
            {
                try
                {
                    await projectManager.Save();
                    await notificationManager.ShowSuccessNotificationAsync($"Project : '{this.PrefabProject.Name}' was saved.");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to save project : '{0}'", this.PrefabProject.Name);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }           
        }      

        #endregion Save project

        protected override async Task CloseAsync()
        {
            try
            {
                await SetSelectedItem(null);
                await this.globalEventAggregator.PublishOnUIThreadAsync(new EditorClosedNotification<PrefabProject>(this.PrefabProject));
                this.globalEventAggregator.Unsubscribe(this);
                await this.TryCloseAsync(true);              
                this.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to close project : '{0}'", this.PrefabProject.Name);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            this.serviceResolver.Dispose();
        }

    }
}
