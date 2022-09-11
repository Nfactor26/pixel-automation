using Ninject.Extensions.Conventions;
using Ninject.Modules;
using System.Reflection;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    internal class NativeModules : NinjectModule
    {
        private ICollection<Assembly> assemblies = new List<Assembly>();

        public NativeModules(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                this.assemblies.Add(assembly);
            }
        }
        public override void Load()
        {
            foreach(var assembly in assemblies)
            {               
                Kernel.Bind(x => x.From(assembly).SelectAllClasses().BindAllInterfaces().Configure(b => b.InSingletonScope()));
            }         
        }
    }
}
