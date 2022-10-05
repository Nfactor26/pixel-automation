using Ninject.Modules;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RunTime;

namespace Pixel.Automation.Test.Runner.Modules
{
    public class ScopedModules : NinjectModule
    {
        public override void Load()
        {           
            Kernel.Bind<IDataSourceReader>().To<DataSourceReader>().InSingletonScope();
            Kernel.Bind<IArgumentProcessor>().To<ArgumentProcessor>().InSingletonScope();
            Kernel.Bind<IPrefabLoader>().To<PrefabLoader>().InSingletonScope(); // since nested prefabs are not supported
            Kernel.Bind<IControlLoader>().To<ControlLoader>().InSingletonScope();
        }
    }
}
