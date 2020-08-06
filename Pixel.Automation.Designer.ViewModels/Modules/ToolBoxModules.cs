using Ninject.Modules;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Designer.ViewModels.Flyouts;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.TestData.Repository.ViewModels;
using Pixel.Automation.TestExplorer.ViewModels;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class ToolBoxModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IToolBox>().To<PropertyGridViewModel>().InSingletonScope();
            Kernel.Bind<IToolBox>().To<ComponentToolBoxViewModel>().InSingletonScope();
            Kernel.Bind<IToolBox>().To<ApplicationExplorerViewModel>().InSingletonScope();           
            Kernel.Bind<IToolBox>().To<TestExplorerViewModel>().InSingletonScope();
            Kernel.Bind<IToolBox>().To<TestDataRepositoryViewModel>().InSingletonScope();
            Kernel.Bind<IFlyOut>().To<SettingsViewModel>().InSingletonScope();

            Kernel.Bind<ApplicationExplorerViewModel>().ToSelf();
            Kernel.Bind<PrefabExplorerViewModel>().ToSelf();
            Kernel.Bind<ControlExplorerViewModel>().ToSelf();
        }
    }
}
