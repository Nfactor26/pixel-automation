using Caliburn.Micro;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Pixel.Scripting.Editor.Core.Contracts;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    internal class CodeGeneratorModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
           .Contains("Pixel.Scripting.CodeGeneration")).SelectAllClasses().InheritedFrom<ICodeGenerator>()
           .BindAllInterfaces().Configure(s => s.InSingletonScope()));
        }
    }
}
