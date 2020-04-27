namespace Pixel.Automation.Core.Devices
{
    public interface IDeviceManager
    {
        /// <summary>
        /// Get specified type of device.
        /// </summary>
        /// <typeparam name="T"></typeparam>       
        /// <returns></returns>
        T AcquireDevice<T>() where T : class, IDevice;
     
    }
}
