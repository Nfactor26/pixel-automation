using Ninject.Modules;
using Pixel.Automation.Arguments.Editor;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Designer.ViewModels.AutomationBuilder;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.TypeBrowser;
using Pixel.Automation.PrefabExplorer.ViewModels;
using Pixel.Automation.RunTime;
using Pixel.Automation.RunTime.DataReader;
using Pixel.Automation.TestDataExplorer.ViewModels;
using Pixel.Automation.TestExplorer.ViewModels;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;

namespace Pixel.Automation.Designer.ViewModels.Modules;

public class WorkspaceModules : NinjectModule
{
    public override void Load()
    {
        Kernel.Bind<IAutomationEditor>().To<AutomationEditorViewModel>().InSingletonScope();       
        Kernel.Bind<ITestExplorer>().To<TestExplorerViewModel>();
        Kernel.Bind<ITestDataRepository>().To<TestDataExplorerViewModel>();
        Kernel.Bind<IPrefabExplorer>().To<PrefabExplorerViewModel>();
        Kernel.Bind<IComponentViewBuilder>().To<ComponentViewBuilder>().InSingletonScope();

        Kernel.Bind<IAutomationProjectManager>().To<AutomationProjectManager>().InSingletonScope();          
        Kernel.Bind<IProjectFileSystem>().To<ProjectFileSystem>().InSingletonScope();      
        Kernel.Bind<IEntityManager>().ToConstructor(c => new EntityManager(c.Inject<IServiceResolver>())).InSingletonScope();

        Kernel.Bind<IDataReader>().To<CsvDataReader>().InSingletonScope();
        Kernel.Bind<IDataSourceReader>().To<DataSourceReader>().InSingletonScope();
        Kernel.Bind<IArgumentTypeProvider>().To<ArgumentTypeProvider>().InSingletonScope();
        Kernel.Bind<IArgumentTypeBrowserFactory>().To<ArgumentTypeBrowserFactory>().InSingletonScope();
        Kernel.Bind<ITestRunner>().To<TestRunner>().InSingletonScope();

        Kernel.Bind<IArgumentProcessor>().To<ArgumentProcessor>().InTransientScope();              
        Kernel.Bind<IControlLoader>().To<DesignTimeControlLoader>().InSingletonScope();

        Kernel.Bind<IProjectAssetsDataManager>().To<ProjectAssetsDataManager>().InSingletonScope();          
    }
}
