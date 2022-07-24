using Ninject.Modules;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Native.Windows;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    internal class WindowsModules : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IApplicationWindowManager>().To<ApplicationWindowManager>().InSingletonScope();
            Kernel.Bind<IProcessManager>().To<ProcessManager>().InSingletonScope();
            Kernel.Bind<IHighlightRectangleFactory>().To<HighlightRectangleFactory>().InSingletonScope();
            Kernel.Bind<IScreenCapture>().To<ScreenCapture>().InSingletonScope();
        }
    }
}
