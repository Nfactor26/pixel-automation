using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Input.Devices
{
    [DataContract]
    [Serializable]
    public abstract class DeviceInputActor : ActorComponent
    {
        protected ISyntheticKeyboard GetKeyboard()
        {
            var keyboard = this.EntityManager.GetServiceOfType<ISyntheticKeyboard>();
            return keyboard;
        }


        protected ISyntheticMouse GetMouse()
        {
            var mouse = this.EntityManager.GetServiceOfType<ISyntheticMouse>();
            return mouse;
        }

        [Browsable(false)]
        protected IControlEntity ControlEntity
        {
            get => this.Parent as IControlEntity;          
        }

        protected DeviceInputActor(string name = "", string tag = ""):base(name,tag)
        {

        }

        internal protected ScreenCoordinate GetScreenCoordinateFromControl(InArgument<UIControl> targetControl)
        {
            var argumentProcessor = this.ArgumentProcessor;
            UIControl control;
            if (targetControl.IsConfigured())
            {
                control = argumentProcessor.GetValue<UIControl>(targetControl);
            }
            else
            {
                ThrowIfMissingControlEntity();
                control = this.ControlEntity.GetControl();
            }
          
            if (control != null)
            {
                control.GetClickablePoint(out double x, out double y);
                return new ScreenCoordinate(x, y);
            }

            throw new ElementNotFoundException("Control could not be located");
        }


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
