using Caliburn.Micro;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Control;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.TestData.Repository.ViewModels;
using Pixel.Automation.TestExplorer.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class AnchorableModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IAnchorable>().To<PropertyGridViewModel>().InSingletonScope();
            Kernel.Bind<IAnchorable>().To<ComponentToolBoxViewModel>().InSingletonScope();        
            Kernel.Bind<IAnchorable>().To<TestExplorerHostViewModel>().InSingletonScope();

            Kernel.Bind<IAnchorable>().To<ApplicationExplorerViewModel>().InSingletonScope();
            Kernel.Bind<IAnchorable>().To<TestDataRepositoryHostViewModel>().InSingletonScope();

            //Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
            //.EndsWith("ViewModels")).SelectAllClasses().InheritedFrom<IToolBox>()
            //.BindAllInterfaces().Configure(s => s.InSingletonScope()));

            Kernel.Bind<IReadOnlyCollection<IAnchorable>>()
            .ToMethod(ctx => new ReadOnlyCollection<IAnchorable>(ctx.Kernel.GetAll<IAnchorable>().ToList())).InSingletonScope();

            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
             .EndsWith("ViewModels")).SelectAllClasses().InheritedFrom<IFlyOut>()
             .BindAllInterfaces().Configure(s => s.InSingletonScope()));


            //Kernel.Bind<IFlyOut>().To<SettingsViewModel>().InSingletonScope();
          
            Kernel.Bind<PrefabExplorerViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind<ControlExplorerViewModel>().ToSelf().InSingletonScope();
        }
    }
}
