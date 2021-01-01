﻿using Ninject.Modules;
using Pixel.Persistence.Services.Client;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class PersistenceModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IMetaDataClient>().To<MetaDataClient>();
            Kernel.Bind<IApplicationRepositoryClient>().To<ApplicationRepositoryClient>();
            Kernel.Bind<IControlRepositoryClient>().To<ControlRepositoryClient>();
            Kernel.Bind<IProjectRepositoryClient>().To<ProjectRepositoryClient>();
            Kernel.Bind<IPrefabRepositoryClient>().To<PrefabRepositoryClient>();
            Kernel.Bind<ITestSessionClient>().To<TestSessionClient>();
            Kernel.Bind<IApplicationDataManager>().To<ApplicationDataManager>().InSingletonScope();
        }
    }
}
