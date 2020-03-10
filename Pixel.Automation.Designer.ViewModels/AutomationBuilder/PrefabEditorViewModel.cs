﻿using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.Diagnostics;

namespace Pixel.Automation.Designer.ViewModels
{
    public class PrefabEditorViewModel : EditorViewModel , IPrefabEditor
    {
        #region data members
      
        private Entity processRoot = default;      

        #endregion data members

        #region constructor
        public PrefabEditorViewModel(IEventAggregator globalEventAggregator, IServiceResolver serviceResolver, ISerializer serializer, IToolBox[] toolBoxes) :
            base(globalEventAggregator, serviceResolver, serializer, toolBoxes)
        {
        }

        #endregion constructor

        #region Automation Project

        PrefabProjectManager projectManager;

        public PrefabDescription PrefabDescription { get; private set; }

       

        public virtual void DoLoad(PrefabDescription prefabDescription)
        {
            Debug.Assert(prefabDescription != null);


            this.projectManager = this.EntityManager.GetServiceOfType<PrefabProjectManager>().WithEntityManager(this.EntityManager) as PrefabProjectManager;


            this.PrefabDescription = prefabDescription;
            this.DisplayName = prefabDescription.PrefabName;
            this.processRoot = this.projectManager.Load(prefabDescription);

            this.EntityManager.RootEntity = this.processRoot;
            this.EntityManager.WorkingDirectory = this.projectManager.GetProjectFileSystem().WorkingDirectory;
            this.WorkFlowRoot = new BindableCollection<Entity>();
            this.WorkFlowRoot.Add(this.processRoot);
            this.BreadCrumbItems.Add(this.processRoot);
        }


        public override async void EditDataModel()
        {
            ICodeEditorScreen codeEditorScreen = this.EntityManager.GetServiceOfType<ICodeEditorScreen>();
            codeEditorScreen.OpenDocument("DataModel.cs", string.Empty);
            IWindowManager windowManager = this.EntityManager.GetServiceOfType<IWindowManager>();
            var result = await windowManager.ShowDialogAsync(codeEditorScreen);
            if (result.HasValue && result.Value)
            {               
                this.projectManager.Refresh();
            }
        }

        #endregion Automation Project     

        #region OnLoad

        //protected override void OnActivate()
        //{
        //    this.testExplorerToolBox?.CloseActiveInstance();
        //    base.OnActivate();
        //}
             
        #endregion OnLoad

        #region Save project

        public override void DoSave()
        {
            projectManager.Save();          
        }

        public override void CreateSnapShot()
        {
            projectManager.CreateSnapShot();
            OnPrefabUpdated();
        }

        public void DoDeploy()
        {
            //Ask for the version to deploy
            //generate the dll from custom sln
            //Package everything           
        }


        #endregion Save project

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
