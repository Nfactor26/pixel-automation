﻿using Ninject.Modules;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RunTime;
using Pixel.Automation.RunTime.DataReader;
using Pixel.Automation.RunTime.Serialization;
using Pixel.OpenId.Authenticator;

namespace Pixel.Automation.Test.Runner.Modules
{
    internal class CommonModule : NinjectModule
    {
        public override void Load()
        {

            Kernel.Bind<ISerializer>().To<JsonSerializer>().InSingletonScope();

            Kernel.Bind<IProjectFileSystem>().To<ProjectFileSystem>().InSingletonScope();
            Kernel.Bind<IPrefabFileSystem>().To<PrefabFileSystem>();
            Kernel.Bind<ITestCaseFileSystem>().To<TestCaseFileSystem>();
            Kernel.Bind<IApplicationFileSystem>().To<ApplicationFileSystem>().InSingletonScope();

            Kernel.Bind<ISignInManager>().To<ClientCredentialSignInManager>().InSingletonScope();

            Kernel.Bind<ITypeProvider>().To<KnownTypeProvider>().InSingletonScope();
            Kernel.Bind<IServiceResolver>().To<ServiceResolver>().InSingletonScope();

           
            Kernel.Bind<IPrefabLoader>().To<PrefabLoader>().InSingletonScope(); // since nested prefabs are not supported          
            Kernel.Bind<IDataReader>().To<CsvDataReader>().InSingletonScope();

            Kernel.Bind<IEntityManager>().ToConstructor(c => new EntityManager(c.Inject<IServiceResolver>())).InSingletonScope();

            Kernel.Bind<ITestRunner>().To<TestRunner>().InSingletonScope();
            Kernel.Bind<ITestSelector>().To<TestSelector>().InSingletonScope();

            Kernel.Bind<TemplateManager>().ToSelf();
        }
    }
}
