using Ninject.Modules;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RunTime;
using Pixel.Automation.RunTime.DataReader;
using Pixel.Automation.RunTime.Serialization;
using Pixel.Persistence.Services.Client;

namespace Pixel.Automation.Test.Runner.Modules
{
    internal class CommonModule : NinjectModule
    {
        public override void Load()
        {

            Kernel.Bind<ISerializer>().To<JsonSerializer>().InSingletonScope();
           
            Kernel.Bind<IProjectFileSystem>().To<ProjectFileSystem>();
            Kernel.Bind<IPrefabFileSystem>().To<PrefabFileSystem>();
            Kernel.Bind<ITestCaseFileSystem>().To<TestCaseFileSystem>();

            Kernel.Bind<ITypeProvider>().To<KnownTypeProvider>().InSingletonScope();
            Kernel.Bind<IServiceResolver>().To<ServiceResolver>();

           
            Kernel.Bind<IPrefabLoader>().To<PrefabLoader>().InSingletonScope(); // since nested prefabs are not supported          
            Kernel.Bind<IDataReader>().To<CsvDataReader>().InSingletonScope();

            Kernel.Bind<IEntityManager>().To<EntityManager>().InSingletonScope();

            Kernel.Bind<ITestSessionClient>().To<TestSessionClient>().InSingletonScope();
        }
    }
}
