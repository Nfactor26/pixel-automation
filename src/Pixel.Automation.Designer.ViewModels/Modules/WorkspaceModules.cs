using Ninject.Modules;
using Pixel.Automation.Arguments.Editor;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.TypeBrowser;
using Pixel.Automation.RunTime;
using Pixel.Automation.RunTime.DataReader;
using Pixel.Automation.TestData.Repository.ViewModels;
using Pixel.Automation.TestExplorer.ViewModels;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class WorkspaceModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IAutomationEditor>().To<AutomationEditorViewModel>().InSingletonScope();
            Kernel.Bind<IPrefabEditor>().To<PrefabEditorViewModel>().InSingletonScope();
            Kernel.Bind<ITestExplorer>().To<TestExplorerViewModel>();
            Kernel.Bind<ITestDataRepository>().To<TestDataRepositoryViewModel>();
            Kernel.Bind<IComponentViewBuilder>().To<ComponentViewBuilder>().InSingletonScope();

            Kernel.Bind<IAutomationProjectManager>().To<AutomationProjectManager>().InSingletonScope();
            Kernel.Bind<IPrefabProjectManager>().To<PrefabProjectManager>().InSingletonScope();
            Kernel.Bind<IProjectFileSystem>().To<ProjectFileSystem>().InSingletonScope();
            Kernel.Bind<IPrefabFileSystem>().To<PrefabFileSystem>();
            Kernel.Bind<IEntityManager>().ToConstructor(c => new EntityManager(c.Inject<IServiceResolver>())).InSingletonScope();

            Kernel.Bind<IDataReader>().To<CsvDataReader>().InSingletonScope();
            Kernel.Bind<IDataSourceReader>().To<DataSourceReader>().InSingletonScope();
            Kernel.Bind<IArgumentTypeProvider>().To<ArgumentTypeProvider>().InSingletonScope();
            Kernel.Bind<IArgumentTypeBrowserFactory>().To<ArgumentTypeBrowserFactory>().InSingletonScope();
            Kernel.Bind<ITestRunner>().To<TestRunner>().InSingletonScope();

            Kernel.Bind<IArgumentProcessor>().To<ArgumentProcessor>().InTransientScope();    
            Kernel.Bind<IPrefabLoader>().To<DesignTimePrefabLoader>().InSingletonScope();
            Kernel.Bind<IControlLoader>().To<DesignTimeControlLoader>().InSingletonScope();          
        }
    }
}
