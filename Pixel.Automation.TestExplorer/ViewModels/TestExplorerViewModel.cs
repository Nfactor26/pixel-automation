using Caliburn.Micro;
using Pixel.Automation.Editor.Core;
using System.Collections.Generic;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class TestExplorerViewModel : ToolBox, ITestExplorer 
    {      
       
        public TestRepositoryManager ActiveInstance { get; set; }

        public override PaneLocation PreferredLocation => PaneLocation.Left;

        public TestDataSourceDropHandler TestDataSourceDropHandler { get; private set; }

        public TestExplorerViewModel(IEventAggregator eventAggregator)
        {
            this.DisplayName = "Test Explorer";
            this.TestDataSourceDropHandler = new TestDataSourceDropHandler(eventAggregator);
        }

        public void SetActiveInstance(object instance)
        {
            this.ActiveInstance = instance as TestRepositoryManager;
            NotifyOfPropertyChange(nameof(ActiveInstance));
            NotifyOfPropertyChange(nameof(IsTestProcessOpen));
        }

        public void CloseActiveInstance()
        {         
            if(this.ActiveInstance != null)
            {
                var openTests = new List<TestCaseViewModel>(this.ActiveInstance.OpenTestCases);
                foreach (var testCase in openTests)
                {
                    this.ActiveInstance.DoneEditing(testCase, false);
                }

                this.ActiveInstance = null;
            }
           
            NotifyOfPropertyChange(nameof(ActiveInstance));
            NotifyOfPropertyChange(nameof(IsTestProcessOpen));
        }

        public bool IsTestProcessOpen
        {
            get => this.ActiveInstance != null;
        }

        public bool HasTestCaseOpenForEdit()
        {
            return this.ActiveInstance?.OpenTestCases.Count > 0;
        }


        protected virtual void Dispose(bool isDisposing)
        {
            if(this.ActiveInstance != null)
            {
                CloseActiveInstance();
            }          
        }
     
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
