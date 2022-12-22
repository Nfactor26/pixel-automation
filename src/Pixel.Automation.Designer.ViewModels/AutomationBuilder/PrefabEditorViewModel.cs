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
            Debug.Assert(prefabProject != null);
   
            this.PrefabProject = prefabProject;
            this.DisplayName = prefabProject.PrefabName;

            var targetVersion = versionToLoad ?? PrefabProject.LatestActiveVersion;
            if (targetVersion != null)
            {
                await this.projectManager.Load(prefabProject, targetVersion);
                UpdateWorkFlowRoot();
                return;
            }

            throw new InvalidDataException($"No active version could be located for project : {this.PrefabProject.PrefabName}");
         
        }

        /// <summary>
        /// Edit the PrefabDataModel that was generated while creating the Prefab.
        /// </summary>
        /// <returns></returns>
        public override async Task EditDataModelAsync()
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
                        await editor.AddDocumentAsync(Path.GetFileName(file), this.PrefabProject.PrefabName, File.ReadAllText(file), false);
                    }
                    await editor.AddDocumentAsync($"{Constants.PrefabDataModelName}.cs", this.PrefabProject.PrefabName, string.Empty, false);
                    await editor.OpenDocumentAsync($"{Constants.PrefabDataModelName}.cs", this.PrefabProject.PrefabName);

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
                await this.Reload();
            }
            catch (Exception ex)
            {
                logger.Information(ex, "There was an error while trying to edit data model for prefab : '{0}'", this.PrefabProject.PrefabName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        protected override async Task Reload()
        {
            await this.projectManager.Reload();
            this.UpdateWorkFlowRoot();
        }

        #endregion Automation Project     

        #region Save project

        public override async Task DoSave()
        {
            try
            {
                await projectManager.Save();
                await notificationManager.ShowSuccessNotificationAsync("Project was saved.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to save project : '{0}'", this.PrefabProject.PrefabName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }    
        }      

        #endregion Save project

        protected override async Task CloseAsync()
        {
            try
            {
                this.Dispose();
                await this.TryCloseAsync(true);
                await this.globalEventAggregator.PublishOnUIThreadAsync(new EditorClosedNotification<PrefabProject>(this.PrefabProject));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to close project : '{0}'", this.PrefabProject.PrefabName);
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
