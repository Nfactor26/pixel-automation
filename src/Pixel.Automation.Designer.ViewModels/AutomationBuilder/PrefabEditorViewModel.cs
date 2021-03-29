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
using System;
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
       
        public PrefabDescription PrefabDescription { get; private set; }       

        public virtual void DoLoad(PrefabDescription prefabDescription, VersionInfo versionToLoad = null)
        {
            Debug.Assert(prefabDescription != null);
   
            this.PrefabDescription = prefabDescription;
            this.DisplayName = prefabDescription.PrefabName;

            var targetVersion = versionToLoad ?? PrefabDescription.ActiveVersion;
            if (targetVersion != null)
            {
                this.projectManager.Load(prefabDescription, targetVersion);
                UpdateWorkFlowRoot();
                return;
            }

            throw new InvalidDataException($"No active version could be located for project : {this.PrefabDescription.PrefabName}");
         
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
                    await editor.AddDocumentAsync(Path.GetFileName(file), this.PrefabDescription.PrefabName, File.ReadAllText(file), false);
                }
                await editor.AddDocumentAsync($"{Constants.PrefabDataModelName}.cs", this.PrefabDescription.PrefabName, string.Empty, false);
                await editor.OpenDocumentAsync($"{Constants.PrefabDataModelName}.cs", this.PrefabDescription.PrefabName);                            
             
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

            var versionManager = this.versionManagerFactory.CreatePrefabVersionManager(this.PrefabDescription);          
            await this.windowManager.ShowDialogAsync(versionManager);

            var fileSystem = this.projectManager.GetProjectFileSystem() as IVersionedFileSystem;
            fileSystem.SwitchToVersion(this.PrefabDescription.ActiveVersion);
        }

        #endregion Save project


        protected override async Task CloseAsync()
        {
            this.Dispose();         
            await this.TryCloseAsync(true);        
        }

        public event EventHandler<PrefabUpdatedEventArgs> PrefabUpdated = delegate { };

        protected virtual void OnPrefabUpdated()
        {
            this.PrefabUpdated(this, new PrefabUpdatedEventArgs(this.PrefabDescription));
        }

        public event EventHandler<EditorClosingEventArgs> EditorClosing = delegate { };

        protected virtual void OnEditorClosing()
        {
            this.EditorClosing(this, new EditorClosingEventArgs());
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            this.serviceResolver.Dispose();
        }

    }
}
