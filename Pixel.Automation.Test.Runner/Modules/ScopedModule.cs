using Ninject.Modules;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RunTime;
using Pixel.Automation.Scripting.Components.Arguments;

namespace Pixel.Automation.Test.Runner.Modules
{
    public class ScopedModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IDataSourceReader>().To<DataSourceReader>().InSingletonScope();
            Kernel.Bind<IArgumentProcessor>().To<ArgumentProcessor>().InSingletonScope();
            Kernel.Bind<ITestRunner>().To<TestRunner>().InSingletonScope();
        }
    }
}
