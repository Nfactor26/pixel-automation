using Caliburn.Micro;
using Dawn;
using GongSolutions.Wpf.DragDrop;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels
{
    public class PrefabEditorViewModel : EditorViewModel , IPrefabEditor
    {
        private readonly IServiceResolver serviceResolver;
        private readonly IPrefabProjectManager projectManager;    
      
        #region constructor

        public PrefabEditorViewModel(IServiceResolver serviceResolver, IEventAggregator globalEventAggregator, IWindowManager windowManager, ISerializer serializer,
             IEntityManager entityManager, IPrefabProjectManager projectManager, IScriptExtactor scriptExtractor, 
             IReadOnlyCollection<IToolBox> tools, IVersionManagerFactory versionManagerFactory, IDropTarget dropTarget, ApplicationSettings applicationSettings) :
            base(globalEventAggregator, windowManager, serializer, entityManager, scriptExtractor, tools, versionManagerFactory, dropTarget, applicationSettings)
        {
            this.serviceResolver = Guard.Argument(serviceResolver, nameof(serviceResolver)).NotNull().Value;
            this.projectManager = Guard.Argument(projectManager, nameof(projectManager)).NotNull().Value;
        }

        #endregion constructor

        #region Automation Project
       
        public PrefabProject PrefabProject { get; private set; }       

        public virtual void DoLoad(PrefabProject prefabProject, VersionInfo versionToLoad = null)
        {
            Debug.Assert(prefabProject != null);
   
            this.PrefabProject = prefabProject;
            this.DisplayName = prefabProject.PrefabName;

            var targetVersion = versionToLoad ?? PrefabProject.ActiveVersion;
            if (targetVersion != null)
            {
                this.projectManager.Load(prefabProject, targetVersion);
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
            var editorFactory = this.EntityManager.GetServiceOfType<ICodeEditorFactory>();
            using (var editor = editorFactory.CreateMultiCodeEditorScreen())
            {
                foreach (var file in Directory.GetFiles(this.projectManager.GetProjectFileSystem().DataModelDirectory, "*.cs"))
                {
                    await editor.AddDocumentAsync(Path.GetFileName(file), this.PrefabProject.PrefabName, File.ReadAllText(file), false);
                }
                await editor.AddDocumentAsync($"{Constants.PrefabDataModelName}.cs", this.PrefabProject.PrefabName, string.Empty, false);
                await editor.OpenDocumentAsync($"{Constants.PrefabDataModelName}.cs", this.PrefabProject.PrefabName);                            
             
                await this.windowManager.ShowDialogAsync(editor);               
            }
            await this.projectManager.Refresh();

            UpdateWorkFlowRoot();
        }

        #endregion Automation Project     

        #region Save project

        public override async Task DoSave()
        {
            await projectManager.Save();           
        }

        public async override Task Manage()
        {
            await DoSave();

            var versionManager = this.versionManagerFactory.CreatePrefabVersionManager(this.PrefabProject);          
            var result  = await this.windowManager.ShowDialogAsync(versionManager);
            if(result.HasValue && result.Value)
            {
                var fileSystem = this.projectManager.GetProjectFileSystem() as IVersionedFileSystem;
                fileSystem.SwitchToVersion(this.PrefabProject.ActiveVersion);

                //This will update Code editor and script editor to point to the new workspace directory
                //This will update Code editor and script editor to point to the new workspace directory
                var codeEditorFactory = this.EntityManager.GetServiceOfType<ICodeEditorFactory>();
                codeEditorFactory.SwitchWorkingDirectory(fileSystem.DataModelDirectory);
                var scriptEditorFactory = this.EntityManager.GetServiceOfType<IScriptEditorFactory>();
                scriptEditorFactory.SwitchWorkingDirectory(fileSystem.WorkingDirectory);
            }         
        }

        #endregion Save project

        protected override async Task CloseAsync()
        {
            this.Dispose();         
            await this.TryCloseAsync(true);        
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            this.serviceResolver.Dispose();
        }

    }
}
