using Caliburn.Micro;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Pixel.Automation.Core;
using Pixel.Scripting.Editor.Core.Contracts;

namespace Pixel.Automation.Designer.ViewModels.Modules
{

    /// <summary>
    /// GlobalScriptingModules are registered at application level Kernel. This is required because components like PrefabBuilder need a new instance each time. Hence, registered as Transient.
    /// </summary>
    internal class GlobalScriptingModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
           .Contains("Pixel.Scripting.Editor.Services.CSharp")).SelectAllClasses().InheritedFrom<IEditorService>()
           .BindAllInterfaces().Configure(s => s.InTransientScope()));

            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
            .StartsWith("Pixel.Scripting.Script.Editor")).SelectAllClasses().InheritedFrom<ICodeEditorFactory>()
            .BindAllInterfaces().Configure(s => s.InTransientScope()));

            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
             .StartsWith("Pixel.Scripting.Script.Editor")).SelectAllClasses().InheritedFrom<IScriptEditorFactory>()
             .BindAllInterfaces().Configure(s => s.InTransientScope()));


            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
             .StartsWith("Pixel.Scripting.Script.Editor")).SelectAllClasses().InheritedFrom<IREPLEditorFactory>()
             .BindAllInterfaces().Configure(s => s.InTransientScope()));

            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
           .StartsWith("Pixel.Scripting.Common.CSharp")).SelectAllClasses().InheritedFrom<IWorkspaceManagerFactory>()
           .BindAllInterfaces().Configure(s => s.InTransientScope()));

            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
           .StartsWith("Pixel.Scripting.Engine.CSharp")).SelectAllClasses().InheritedFrom<IScriptEngineFactory>()
           .BindAllInterfaces().Configure(s => s.InTransientScope()));

        }
    }

    /// <summary>
    /// ScriptingModules are registered at child kernel levevls which are responsible for resolving dependencies in a workspace which should be always same within a workspace. Hence, registered as singleton.
    /// </summary>
    internal class ScriptingModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
           .Contains("Pixel.Scripting.Editor.Services.CSharp")).SelectAllClasses().InheritedFrom<IEditorService>()
           .BindAllInterfaces().Configure(s => s.InTransientScope()));

            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
            .StartsWith("Pixel.Scripting.Script.Editor")).SelectAllClasses().InheritedFrom<ICodeEditorFactory>()
            .BindAllInterfaces().Configure(s => s.InSingletonScope()));

            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
             .StartsWith("Pixel.Scripting.Script.Editor")).SelectAllClasses().InheritedFrom<IScriptEditorFactory>()
             .BindAllInterfaces().Configure(s => s.InSingletonScope()));


            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
             .StartsWith("Pixel.Scripting.Script.Editor")).SelectAllClasses().InheritedFrom<IREPLEditorFactory>()
             .BindAllInterfaces().Configure(s => s.InSingletonScope()));

            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
           .StartsWith("Pixel.Scripting.Common.CSharp")).SelectAllClasses().InheritedFrom<IWorkspaceManagerFactory>()
           .BindAllInterfaces().Configure(s => s.InTransientScope()));

            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
           .StartsWith("Pixel.Scripting.Engine.CSharp")).SelectAllClasses().InheritedFrom<IScriptEngineFactory>()
           .BindAllInterfaces().Configure(s => s.InSingletonScope()));

        }
    }
}
