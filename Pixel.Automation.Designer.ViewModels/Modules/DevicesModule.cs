using Ninject.Modules;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Native.Windows.Device;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class DevicesModule : NinjectModule
    {
        public override void Load()
        {            
            Kernel.Bind<ISyntheticMouse>().To<SyntheticMouse>().InSingletonScope();
            Kernel.Bind<ISyntheticKeyboard>().To<SyntheticKeyboard>().InSingletonScope();
        }
    }
}
