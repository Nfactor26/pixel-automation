﻿using Caliburn.Micro;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Pixel.Automation.Core;
using Pixel.Scripting.Editor.Core.Contracts;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    internal class ScriptingModule : NinjectModule
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
              .BindAllInterfaces().Configure(s => s.InTransientScope()));

            // Kernel.Bind(x => x.FromAssembliesInPath("Editors", a => a.GetAssemblyName()
            // .StartsWith("Pixel")).SelectAllClasses().InheritedFrom<IEditorService>()
            // .BindAllInterfaces().Configure(s => s.InTransientScope()));

            // Kernel.Bind(x => x.FromAssembliesInPath("Editors", a => a.GetAssemblyName()
            // .StartsWith("Pixel")).SelectAllClasses().InheritedFrom<ICodeEditorFactory>()
            // .BindAllInterfaces().Configure(s => s.InSingletonScope()));

            // Kernel.Bind(x => x.FromAssembliesInPath("Editors", a => a.GetAssemblyName()
            //  .StartsWith("Pixel")).SelectAllClasses().InheritedFrom<IScriptEditorFactory>()
            //  .BindAllInterfaces().Configure(s => s.InSingletonScope()));


            // Kernel.Bind(x => x.FromAssembliesInPath("Editors", a => a.GetAssemblyName()
            //  .StartsWith("Pixel")).SelectAllClasses().InheritedFrom<IREPLEditorFactory>()
            //  .BindAllInterfaces().Configure(s => s.InSingletonScope()));

            // Kernel.Bind(x => x.FromAssembliesInPath("Editors", a => a.GetAssemblyName()
            //.StartsWith("Pixel")).SelectAllClasses().InheritedFrom<IWorkspaceManagerFactory>()
            //.BindAllInterfaces().Configure(s => s.InTransientScope()));

            // Kernel.Bind(x => x.FromAssembliesInPath("Core", a => a.GetAssemblyName()
            //   .StartsWith("Pixel")).SelectAllClasses().InheritedFrom<IScriptEngineFactory>()
            //   .BindAllInterfaces().Configure(s => s.InTransientScope()));
        }
    }
}
