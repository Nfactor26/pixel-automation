using Dawn;
using Pixel.Automation.Editor.Core;
using System.Linq;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class TestExplorerViewModel : ToolBox, ITestExplorer 
    {
        private readonly object locker = new object();

        public TestRepositoryManager ActiveInstance { get; set; }

        public override PaneLocation PreferredLocation => PaneLocation.Left;
     
        public TestExplorerViewModel()
        {
            this.DisplayName = "Test Explorer";           
        }

        public void SetActiveInstance(object instance)
        {
            Guard.Argument(instance).NotNull().Compatible<TestRepositoryManager>();
            lock (locker)
            {
                this.ActiveInstance = instance as TestRepositoryManager;
            }
         
            NotifyOfPropertyChange(nameof(ActiveInstance));
            NotifyOfPropertyChange(nameof(IsTestProcessOpen));
        }

        public void ClearActiveInstance()
        { 
            lock(locker)
            {
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
            return this.ActiveInstance?.TestFixtures.Any(f => f.IsOpenForEdit) ?? false;
        }


        protected virtual void Dispose(bool isDisposing)
        {
            ClearActiveInstance();
        }
     
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
