using Caliburn.Micro;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editors.Image.Capture;
using Pixel.Automation.Native.Windows;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class ScrappersModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IImageCaptureViewModel>().To<ImageCaptureViewModel>();

            Kernel.Bind(x => x.FromAssembliesInPath(".", a => a.GetAssemblyName()
            .Contains("Scrapper")).SelectAllClasses().InheritedFrom<IControlScrapper>()
            .BindAllInterfaces().Configure(s => s.InSingletonScope()));
        }
    }
}
