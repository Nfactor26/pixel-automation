﻿using Caliburn.Micro;
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
using Pixel.Scripting.Editor.Core.Contracts;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Pixel.Automation.Designer.ViewModels
{

    public class AutomationBuilderViewModel :  EditorViewModel , IAutomationBuilder
    {
        #region data members        
      
        private readonly ITestExplorer testExplorerToolBox;
        private readonly TestDataRepositoryViewModel testDataRepositoryViewModel;
     
        Entity processRoot = default;
        TestRepositoryManager testCaseManager = default;
        TestDataRepository testDataRepository = default;

        #endregion data members

        #region constructor
        public AutomationBuilderViewModel(IEventAggregator globalEventAggregator, IServiceResolver serviceResolver, ISerializer serializer,
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

        public virtual void DoLoad(AutomationProject project, VersionInfo versionToLoad = null)
        {
            Debug.Assert(project != null);

            this.projectManager = this.EntityManager.GetServiceOfType<AutomationProjectManager>().WithEntityManager(this.EntityManager) as AutomationProjectManager;

            this.CurrentProject = project;
            this.DisplayName = project.Name;

            //Always open the most recent non-deployed version if no version is specified
            var targetVersion = versionToLoad ?? project.ActiveVersion;
            if(targetVersion != null)
            {
                this.processRoot = this.projectManager.Load(project, targetVersion);

                this.EntityManager.RootEntity = this.processRoot;
                this.WorkFlowRoot = new BindableCollection<Entity>();
                this.WorkFlowRoot.Add(this.processRoot);
                this.BreadCrumbItems.Add(this.processRoot);
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

            var testCaseEntities = this.EntityManager.RootEntity.GetComponentsByTag("TestCase", SearchScope.Descendants);
            this.projectManager.Refresh();
            this.ReOpenTestCases(testCaseEntities);
        }

        private void ReOpenTestCases(IEnumerable<IComponent> testCaseEntities)
        {
            Entity parentEntity = testCaseEntities.FirstOrDefault()?.Parent;
            foreach (var testEntity in testCaseEntities)
            {
                testEntity.Parent.RemoveComponent(testEntity);
                this.testCaseManager.DoneEditing(testEntity.Tag);
            }

            foreach (var testEntity in testCaseEntities)
            {
                this.testCaseManager.OpenForEdit(testEntity.Tag);
            }
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
            this.testExplorerToolBox?.SetActiveInstance(this.testCaseManager);         
            this.testDataRepositoryViewModel?.SetActiveInstance(this.testDataRepository);
            await base.OnActivateAsync(cancellationToken);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            this.testExplorerToolBox?.CloseActiveInstance();
            this.testDataRepositoryViewModel?.CloseActiveInstance();
            await base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion OnLoad

        #region Save project

        public override void DoSave()
        {
            projectManager.Save();
        }

        public async override Task Manage()
        {
            DoSave();

            var workspaceManagerFactory = this.EntityManager.GetServiceOfType<IWorkspaceManagerFactory>();
            ProjectVersionManagerViewModel versionManager = new ProjectVersionManagerViewModel(this.CurrentProject, workspaceManagerFactory, this.serializer);
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
            if (result == MessageBoxResult.OK)
            {
                await CloseAsync();
            }
        }

        public override async  Task CloseAsync()
        {
            this.Dispose();
            this.testExplorerToolBox?.CloseActiveInstance();          
            this.testDataRepositoryViewModel?.CloseActiveInstance();
            var shell = IoC.Get<IShell>();
            await this.TryCloseAsync(true);
            await(shell as ShellViewModel).DeactivateItemAsync(this, true, CancellationToken.None);
        }

        #endregion Close Screen

      
    }
}
