namespace Pixel.Automation.Core.Devices
{
    public interface IDevice
    {
        /// <summary>
        /// If a device is a critical resource, request for these devices to device manager will be controlled i.e. device manager will provice
        /// access to these device to only one thread at a time.
        /// </summary>
        bool IsCriticalResource { get; }
        
    }
}
