using Ninject.Modules;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RunTime;
using Pixel.Automation.RunTime.Serialization;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class UtilityModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<ISerializer>().To<JsonSerializer>().InSingletonScope();
            Kernel.Bind<ITypeProvider>().To<KnownTypeProvider>().InSingletonScope();
            Kernel.Bind<IServiceResolver>().To<ServiceResolver>();

            Kernel.Bind<IProjectFileSystem>().To<ProjectFileSystem>();
            Kernel.Bind<IPrefabFileSystem>().To<PrefabFileSystem>();           
        }
    }
}
