using Ninject.Modules;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Utilities;
using Pixel.Automation.Native.Windows.Device;

namespace Pixel.Automation.Designer.ViewModels.Modules
{
    public class DevicesModule : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IDeviceManager>().To<DeviceManager>().InSingletonScope();
            Kernel.Bind<IDevice>().To<SyntheticMouse>().InSingletonScope();
            Kernel.Bind<IDevice>().To<SyntheticKeyboard>().InSingletonScope();
        }
    }
}
