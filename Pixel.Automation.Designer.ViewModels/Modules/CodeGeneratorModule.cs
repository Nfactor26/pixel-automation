using Caliburn.Micro;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Pixel.Automation.Core.Interfaces.Scripting;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    internal class CodeGeneratorModule : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
           .Contains("Pixel.Scripting.CodeGeneration")).SelectAllClasses().InheritedFrom<ICodeGenerator>()
           .BindAllInterfaces().Configure(s => s.InTransientScope()));

            // Kernel.Bind(x => x.FromAssembliesInPath("Core", a => a.GetAssemblyName()
            //.Contains("CodeGeneration")).SelectAllClasses().InheritedFrom<ICodeGenerator>()
            //.BindAllInterfaces().Configure(s => s.InTransientScope()));

        }
    }
}
