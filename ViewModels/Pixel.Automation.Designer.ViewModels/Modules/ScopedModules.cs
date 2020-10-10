using Caliburn.Micro;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Scripting.Components.Arguments;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    /// <summary>   
    /// These modules should be same across components managed by a given entity manager.
    /// Primary as well as Seconary EntityManagers will both be configured to use ScopedModules.
    /// </summary>
    public class ScopedModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IArgumentProcessor>().To<ArgumentProcessor>().InTransientScope();

            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
            .StartsWith("Pixel.Scripting.Engine.CSharp")).SelectAllClasses().InheritedFrom<IScriptEngineFactory>()
            .BindAllInterfaces().Configure(s => s.InSingletonScope()));
        }
    }
}
