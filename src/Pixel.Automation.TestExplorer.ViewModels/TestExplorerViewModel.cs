using Dawn;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class TestExplorerViewModel : ToolBox, ITestExplorer 
    {
        private readonly object locker = new object();

        private ITestRepositoryManager activeInstance;
        public ITestRepositoryManager ActiveInstance
        {
            get => this.activeInstance;
            set
            {
                this.activeInstance = value;
                NotifyOfPropertyChange(nameof(ActiveInstance));
            }
        }
        public bool IsTestProcessOpen
        {
            get => this.ActiveInstance != null;
        }

        public override PaneLocation PreferredLocation => PaneLocation.Left;
     
        public TestExplorerViewModel()
        {
            this.DisplayName = "Test Explorer";           
        }

        public void SetActiveInstance(ITestRepositoryManager instance)
        {
            Guard.Argument(instance).NotNull().Compatible<TestRepositoryManager>();
            lock (locker)
            {
                this.ActiveInstance = instance as TestRepositoryManager;
            }        
         
            NotifyOfPropertyChange(nameof(IsTestProcessOpen));
        }

        public void ClearActiveInstance()
        { 
            lock(locker)
            {
                this.ActiveInstance = null;               
            }         
            NotifyOfPropertyChange(nameof(IsTestProcessOpen));
        } 

    }
}
