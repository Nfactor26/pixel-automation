using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Pixel.Automation.Core.Interfaces;
using System.Reflection;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class ScrappersModules : NinjectModule
    {
        private ICollection<Assembly> assemblies = new List<Assembly>();

        public ScrappersModules(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                this.assemblies.Add(assembly);
            }
        }

        public override void Load()
        {
            foreach (var assembly in this.assemblies)
            {
                Kernel.Bind(x => x.From(assembly).SelectAllClasses().InheritedFrom<IControlScrapper>()
                        .BindAllInterfaces().Configure(s => s.InSingletonScope()));
            }
            this.assemblies.Clear();           
        }
    }
}
