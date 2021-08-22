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
    public class ToolBoxModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IToolBox>().To<PropertyGridViewModel>().InSingletonScope();
            Kernel.Bind<IToolBox>().To<ComponentToolBoxViewModel>().InSingletonScope();        
            Kernel.Bind<IToolBox>().To<TestExplorerHostViewModel>().InSingletonScope();

            Kernel.Bind<IToolBox>().To<ApplicationExplorerViewModel>().InSingletonScope();
            Kernel.Bind<IToolBox>().To<TestDataRepositoryViewModel>().InSingletonScope();

            //Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
            //.EndsWith("ViewModels")).SelectAllClasses().InheritedFrom<IToolBox>()
            //.BindAllInterfaces().Configure(s => s.InSingletonScope()));

            Kernel.Bind<IReadOnlyCollection<IToolBox>>()
            .ToMethod(ctx => new ReadOnlyCollection<IToolBox>(ctx.Kernel.GetAll<IToolBox>().ToList())).InSingletonScope();

            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
             .EndsWith("ViewModels")).SelectAllClasses().InheritedFrom<IFlyOut>()
             .BindAllInterfaces().Configure(s => s.InSingletonScope()));


            //Kernel.Bind<IFlyOut>().To<SettingsViewModel>().InSingletonScope();
          
            Kernel.Bind<PrefabExplorerViewModel>().ToSelf();
            Kernel.Bind<ControlExplorerViewModel>().ToSelf();
        }
    }
}
