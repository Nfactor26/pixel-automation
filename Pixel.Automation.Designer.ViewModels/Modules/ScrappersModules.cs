﻿using Caliburn.Micro;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Native.Windows;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class ScrappersModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
            .Contains("Scrapper")).SelectAllClasses().InheritedFrom<IControlScrapper>()
            .BindAllInterfaces().Configure(s => s.InSingletonScope()));
        }
    }
}