using Ninject.Modules;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;

namespace Pixel.Automation.Test.Runner.Modules
{
    public class PersistenceModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IRestClientFactory>().To<RestClientFactory>().InSingletonScope();           
            Kernel.Bind<IApplicationRepositoryClient>().To<ApplicationRepositoryClient>();
            Kernel.Bind<IControlRepositoryClient>().To<ControlRepositoryClient>();
            Kernel.Bind<IAutomationsRepositoryClient>().To<AutomationsRepositoryClient>();
            Kernel.Bind<IPrefabsRepositoryClient>().To<PrefabsRepositoryClient>();
            Kernel.Bind<IReferencesRepositoryClient>().To<ReferencesRepositoryClient>();
            Kernel.Bind<IProjectFilesRepositoryClient>().To<ProjectFilesRepositoryClient>();
            Kernel.Bind<IPrefabFilesRepositoryClient>().To<PrefabFilesRepositoryClient>();
            Kernel.Bind<IFixturesRepositoryClient>().To<FixturesRepositoryClient>();
            Kernel.Bind<ITestsRepositoryClient>().To<TestsRepositoryClient>();
            Kernel.Bind<ITestDataRepositoryClient>().To<TestDataRepositoryClient>();
            Kernel.Bind<ITestSessionClient>().To<TestSessionClient>();
            Kernel.Bind<ITemplateClient>().To<TemplateClient>().InSingletonScope();
            Kernel.Bind<IApplicationDataManager>().To<ApplicationDataManager>().InSingletonScope();
            Kernel.Bind<IProjectDataManager>().To<ProjectDataManager>().InSingletonScope();
            Kernel.Bind<IPrefabDataManager>().To<PrefabDataManager>().InSingletonScope();
        }
    }
}
