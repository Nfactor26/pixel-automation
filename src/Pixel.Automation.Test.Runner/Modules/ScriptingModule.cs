using Ninject.Modules;
using Pixel.Automation.Core;
using Pixel.Scripting.Engine.CSharp;

namespace Pixel.Automation.Test.Runner.Modules
{
    internal class ScriptingModule : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IScriptEngineFactory>().To<ScriptEngineFactory>().InSingletonScope();
        }
    }
}
