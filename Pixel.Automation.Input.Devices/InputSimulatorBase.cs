using Pixel.Automation.Core;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Exceptions;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Input.Devices
{
    [DataContract]
    [Serializable]
    public abstract class InputSimulatorBase : ActorComponent
    {
        protected ISyntheticKeyboard GetKeyboard()
        {
            var deviceManager = this.EntityManager.GetServiceOfType<IDeviceManager>();
            return deviceManager.AcquireDevice<ISyntheticKeyboard>();
        }


        protected ISyntheticMouse GetMouse()
        {
            var deviceManager = this.EntityManager.GetServiceOfType<IDeviceManager>();
            return deviceManager.AcquireDevice<ISyntheticMouse>();
        }

        [Browsable(false)]
        protected ControlEntity ControlEntity
        {
            get => this.Parent as ControlEntity;          
        }

        protected InputSimulatorBase(string name = "", string tag = ""):base(name,tag)
        {

        }

        public override void Act() { }

        protected void ThrowIfMissingControlEntity()
        {
            if (this.ControlEntity == null)
            {
                throw new ConfigurationException($"{nameof(ControlEntity)} is required as a parent entity for component with id :{this.Id}");
            }
        }      

        protected bool IsKeySequenceValid(string keyCode)
        {
            ISyntheticKeyboard syntheticKeyboard = GetKeyboard();
            try
            {
                var syntheticKeySequence = syntheticKeyboard.GetSynthethicKeyCodes(keyCode);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
