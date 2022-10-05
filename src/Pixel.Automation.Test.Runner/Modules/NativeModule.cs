using Ninject.Modules;
using System.Collections.Generic;
using System.Reflection;
using Ninject.Extensions.Conventions;

namespace Pixel.Automation.Test.Runner.Modules
{
    internal class NativeModule : NinjectModule
    {
        private ICollection<Assembly> assemblies = new List<Assembly>();

        public NativeModule(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                this.assemblies.Add(assembly);
            }
        }
        public override void Load()
        {
            foreach (var assembly in assemblies)
            {
                Kernel.Bind(x => x.From(assembly).SelectAllClasses().BindAllInterfaces().Configure(b => b.InSingletonScope()));
            }
        }
    }
}
