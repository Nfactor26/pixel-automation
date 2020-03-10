using Pixel.Automation.Core.Devices;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Utilities
{
    public class DeviceManager : IDeviceManager
    {
        private readonly IEnumerable<IDevice> allDevices;
      
        public DeviceManager(IEnumerable<IDevice> availableDevices)
        {
            this.allDevices = availableDevices;           
        }

        public T AcquireDevice<T>() where T : class, IDevice
        {
            foreach(IDevice device in this.allDevices)
            {
                if (device is T)
                    return device as T;
            }
            throw new InvalidOperationException($"Device of type {typeof(T)} is not available");
        }      

    }
}
