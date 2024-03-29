﻿using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Pixel.Automation.Core.Interfaces;
using System.Reflection;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    internal class NativeModules : NinjectModule
    {
        private List<Assembly> assemblies = new ();

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
                //HighlightRectangle instance must be created on UIThread or it won't display
                _ = Kernel.Get<IHighlightRectangle>();
            }         
        }
    }
}
