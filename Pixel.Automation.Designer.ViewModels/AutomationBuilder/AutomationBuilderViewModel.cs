using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.TestData.Repository.ViewModels;
using Pixel.Automation.TestExplorer;
using Pixel.Scripting.Editor.Core.Contracts;
using System.Collections.Generic;
using System.Diagnostics;
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
        public AutomationBuilderViewModel(IEventAggregator globalEventAggregator, IServiceResolver serviceResolver, ISerializer serializer, IToolBox[] toolBoxes) : base(
            globalEventAggregator, serviceResolver, serializer, toolBoxes)
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

        public virtual void DoLoad(AutomationProject project)
        {
            Debug.Assert(project != null);


            this.projectManager = this.EntityManager.GetServiceOfType<AutomationProjectManager>().WithEntityManager(this.EntityManager) as AutomationProjectManager;


            this.CurrentProject = project;
            this.DisplayName = project.Name;         
            this.processRoot = this.projectManager.Load(project);          
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
                var testCaseEntities = this.EntityManager.RootEntity.GetComponentsByTag("TestCase", SearchScope.Descendants);
                this.projectManager.Refresh();
                this.ReOpenTestCases(testCaseEntities);
            }
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

                this.testCaseManager = new TestRepositoryManager(eventAggregator,this.projectManager, this.projectManager.GetProjectFileSystem() as IProjectFileSystem, this.serializer, testRunner, windowManager);
            }
            this.testExplorerToolBox?.SetActiveInstance(this.testCaseManager);

            if(this.testDataRepository == null)
            {
                this.testDataRepository = this.EntityManager.GetServiceOfType<TestDataRepository>();
                this.testDataRepository.DataSourceChanged += OnTestDataSourceChanged;
            }          
            this.testDataRepositoryViewModel.SetActiveInstance(this.testDataRepository);
        }

        private void OnTestDataSourceChanged(object sender, string e)
        {         
            var testCaseEntities = this.EntityManager.RootEntity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants);
            this.projectManager.Refresh();
            this.ReOpenTestCases(testCaseEntities);
        }

        #endregion Automation Project


        #region OnLoad

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        { 
            switch (this.CurrentProject.AutomationProjectType)
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

        public override void CreateSnapShot()
        {
            projectManager.CreateSnapShot();
        }

        public void DoDeploy()
        {
            //Ask for the version to deploy
            //generate the dll from custom sln
            //Package everything           
        }


        #endregion Save project

        #region Close Screen

        public override bool CanClose()
        {
            return true;
        }

        public override async void CloseScreen()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close? Any unsaved changes will be lost.", "Confirm Close", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                this.Dispose();
                this.testExplorerToolBox?.CloseActiveInstance();
                this.testDataRepository.DataSourceChanged -= OnTestDataSourceChanged;
                this.testDataRepositoryViewModel?.CloseActiveInstance();
                var shell = IoC.Get<IShell>();
                await this.TryCloseAsync(true);
                (shell as ShellViewModel).DeactivateItemAsync(this, true, CancellationToken.None);
            }
        }

        #endregion Close Screen

      
    }
}
