﻿using Caliburn.Micro;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels
{
    public class PrefabEditorViewModel : EditorViewModel , IPrefabEditor
    {
        #region data members
      
        private Entity processRoot = default;      

        #endregion data members

        #region constructor
        public PrefabEditorViewModel(IEventAggregator globalEventAggregator, IServiceResolver serviceResolver, ISerializer serializer,
             IScriptExtactor scriptExtractor, IToolBox[] toolBoxes) :
            base(globalEventAggregator, serviceResolver, serializer, scriptExtractor, toolBoxes)
        {
        }

        #endregion constructor

        #region Automation Project

        PrefabProjectManager projectManager;

        public PrefabDescription PrefabDescription { get; private set; }       

        public virtual void DoLoad(PrefabDescription prefabDescription, VersionInfo versionToLoad = null)
        {
            Debug.Assert(prefabDescription != null);

            this.projectManager = this.EntityManager.GetServiceOfType<PrefabProjectManager>().WithEntityManager(this.EntityManager) as PrefabProjectManager;

            this.PrefabDescription = prefabDescription;
            this.DisplayName = prefabDescription.PrefabName;

            var targetVersion = versionToLoad ?? PrefabDescription.ActiveVersion;
            if (targetVersion != null)
            {
                this.processRoot = this.projectManager.Load(prefabDescription, targetVersion);

                this.EntityManager.RootEntity = this.processRoot;
                this.WorkFlowRoot = new BindableCollection<Entity>();
                this.WorkFlowRoot.Add(this.processRoot);
                this.BreadCrumbItems.Add(this.processRoot);
                return;
            }

            throw new InvalidDataException($"No active version could be located for project : {this.PrefabDescription.PrefabName}");
         
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
                await editor.AddDocumentAsync($"{Constants.PrefabDataModelName}.cs", string.Empty, false);
                await editor.OpenDocumentAsync($"{Constants.PrefabDataModelName}.cs");
                            
                IWindowManager windowManager = this.EntityManager.GetServiceOfType<IWindowManager>();
                await windowManager.ShowDialogAsync(editor);               
            }
            this.projectManager.Refresh();
        }

        #endregion Automation Project     

        #region Save project

        public override void DoSave()
        {
            projectManager.Save();          
        }

        public async override Task Manage()
        {
            DoSave();

            var workspaceManagerFactory = this.EntityManager.GetServiceOfType<IWorkspaceManagerFactory>();
            PrefabVersionManagerViewModel versionManager = new PrefabVersionManagerViewModel(this.PrefabDescription, workspaceManagerFactory, this.serializer);
            IWindowManager windowManager = this.EntityManager.GetServiceOfType<IWindowManager>();
            await windowManager.ShowDialogAsync(versionManager);

            var fileSystem = this.projectManager.GetProjectFileSystem() as IVersionedFileSystem;
            fileSystem.SwitchToVersion(this.PrefabDescription.ActiveVersion);
        }

        #endregion Save project


        public override async Task CloseAsync()
        {
            this.Dispose();           
            var shell = IoC.Get<IShell>();
            await this.TryCloseAsync(true);
            await (shell as ShellViewModel).DeactivateItemAsync(this, true, CancellationToken.None);
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

    }
}
