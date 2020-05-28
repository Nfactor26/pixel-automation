using Ninject.Modules;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Native.Windows;

namespace Pixel.Automation.Test.Runner.Modules
{
    internal class WindowsModule : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IApplicationWindowManager>().To<ApplicationWindowManager>().InSingletonScope();
            Kernel.Bind<IHighlightRectangleFactory>().To<HighlightRectangleFactory>().InSingletonScope();
            Kernel.Bind<IScreenCapture>().To<ScreenCapture>().InSingletonScope();
        }
    }
}
