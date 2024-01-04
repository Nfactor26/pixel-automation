using Ninject.Modules;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;

namespace Pixel.Automation.Designer.ViewModels.Modules;

public class PersistenceModules : NinjectModule
{
    public override void Load()
    {
        Kernel.Bind<IRestClientFactory>().To<RestClientFactory>().InSingletonScope();          
        Kernel.Bind<IApplicationRepositoryClient>().To<ApplicationRepositoryClient>().InSingletonScope();
        Kernel.Bind<IControlRepositoryClient>().To<ControlRepositoryClient>().InSingletonScope();          
        Kernel.Bind<ITestSessionClient>().To<TestSessionClient>().InSingletonScope();
        Kernel.Bind<IAutomationsRepositoryClient>().To<AutomationsRepositoryClient>().InSingletonScope();
        Kernel.Bind<IPrefabsRepositoryClient>().To<PrefabsRepositoryClient>().InSingletonScope();
        Kernel.Bind<IReferencesRepositoryClient>().To<ReferencesRepositoryClient>().InSingletonScope();
        Kernel.Bind<IProjectFilesRepositoryClient>().To<ProjectFilesRepositoryClient>().InSingletonScope();
        Kernel.Bind<IPrefabFilesRepositoryClient>().To<PrefabFilesRepositoryClient>().InSingletonScope();
        Kernel.Bind<IFixturesRepositoryClient>().To<FixturesRepositoryClient>().InSingletonScope();
        Kernel.Bind<ITestsRepositoryClient>().To<TestsRepositoryClient>().InSingletonScope();
        Kernel.Bind<ITestDataRepositoryClient>().To<TestDataRepositoryClient>().InSingletonScope();
     
        Kernel.Bind<IDataManagerFactory>().To<DataManagerFactory>().InSingletonScope();
        Kernel.Bind<IApplicationDataManager>().To<ApplicationDataManager>().InSingletonScope();
        Kernel.Bind<IProjectDataManager>().To<ProjectDataManager>().InSingletonScope();
        Kernel.Bind<IPrefabDataManager>().To<PrefabDataManager>().InSingletonScope();
    }
}
