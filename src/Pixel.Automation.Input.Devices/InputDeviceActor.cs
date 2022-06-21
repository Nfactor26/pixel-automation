using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices
{
    /// <summary>
    /// Base class for Input Device Actor
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class InputDeviceActor : ActorComponent
    {
        /// <summary>
        /// Get <see cref="ISyntheticKeyboard"/> from the entity manager
        /// </summary>
        /// <returns></returns>
        protected ISyntheticKeyboard GetKeyboard()
        {
            var keyboard = this.EntityManager.GetServiceOfType<ISyntheticKeyboard>();
            return keyboard;
        }

        /// <summary>
        /// Get <see cref="ISyntheticMouse"/> from the entity manager
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        protected InputDeviceActor(string name = "", string tag = ""):base(name,tag)
        {

        }

        /// <summary>
        /// Retrieve the control based on whether parent control entity or a target control provided as Argument is configured and then
        /// get the ClickablePoint of this control.
        /// </summary>
        /// <param name="targetControl"></param>
        /// <returns></returns>
        internal protected async Task<ScreenCoordinate> GetScreenCoordinateFromControl(InArgument<UIControl> targetControl)
        {
            var argumentProcessor = this.ArgumentProcessor;
            UIControl control;
            if (targetControl.IsConfigured())
            {
                control = await argumentProcessor.GetValueAsync<UIControl>(targetControl);
            }
            else
            {
                ThrowIfMissingControlEntity();
                control = await this.ControlEntity.GetControl();
            }
          
            if (control != null)
            {
                var (x,y) = await control.GetClickablePointAsync();
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

        /// <summary>
        /// Validate whether the input key sequence is valid
        /// </summary>
        /// <param name="keyCode"></param>
        /// <returns></returns>
        protected bool IsKeySequenceValid(string keyCode)
        {
            try
            {
                ISyntheticKeyboard syntheticKeyboard = GetKeyboard();
                var syntheticKeySequence = syntheticKeyboard.GetSynthethicKeyCodes(keyCode);
                return syntheticKeySequence.Any();
            }
            catch (Exception ex)
            {
                Log.Warning("Key Sequence is not valid. {Message}", ex.Message);
                return false;
            }
        }

    }
}
