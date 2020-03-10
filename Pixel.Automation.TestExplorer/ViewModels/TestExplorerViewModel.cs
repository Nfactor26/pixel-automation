using Caliburn.Micro;
using Pixel.Automation.Editor.Core;

namespace Pixel.Automation.TestExplorer
{
    public class TestExplorerViewModel : ToolBox, ITestExplorer 
    {      
       
        public TestCaseManager ActiveInstance { get; set; }

        public override PaneLocation PreferredLocation => PaneLocation.Left;

        public TestDataSourceDropHandler TestDataSourceDropHandler { get; private set; }

        public TestExplorerViewModel(IEventAggregator eventAggregator)
        {
            this.DisplayName = "Test Explorer";
            this.TestDataSourceDropHandler = new TestDataSourceDropHandler(eventAggregator);
        }

        public void SetActiveInstance(object instance)
        {
            if (instance is TestCaseManager)
            {
                this.ActiveInstance = instance as TestCaseManager;
                NotifyOfPropertyChange(nameof(ActiveInstance));
                NotifyOfPropertyChange(nameof(IsTestProcessOpen));
            }
        }

        public void CloseActiveInstance()
        {
            this.ActiveInstance = null;
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
              
    }
}
