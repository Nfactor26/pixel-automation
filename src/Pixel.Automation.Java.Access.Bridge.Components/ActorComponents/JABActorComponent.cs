using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Exceptions;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    /// <summary>
    /// Base class for actor components based on Java Access Bridge
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class JABActorComponent : ActorComponent
    {

        /// <summary>
        /// If the control to be interacted has been already looked up , it can be specified as an argument.
        /// TargetControl Argument takes precedence over parent Control Entity.
        /// </summary>
        [DataMember]
        [Display(Name = "Target Control", GroupName = "Control Details", Order = 10, Description = "[Optional] Specify a JavaUIControl to be interacted with.")]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };
              
        /// <summary>
        /// Parent Control entity component if any
        /// </summary>
        [Browsable(false)]
        public JavaControlEntity ControlEntity
        {
            get
            {
                return this.Parent as JavaControlEntity;

            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        protected JABActorComponent(string name = "", string tag = "") : base(name, tag)
        {

        }

        /// <summary>
        /// Retrieve the target control specified either as an <see cref="Argument"/> or a parent <see cref="JavaControlEntity"/>
        /// </summary>
        /// <returns>Control as <see cref="AccessibleContextNode"/></returns>
        protected async Task<AccessibleContextNode> GetTargetControl()
        {
            UIControl targetControl;
            if (this.TargetControl.IsConfigured())
            {
                targetControl = await ArgumentProcessor.GetValueAsync<UIControl>(this.TargetControl);
            }
            else
            {
                ThrowIfMissingControlEntity();
                targetControl = await this.ControlEntity.GetControl();
            }

            AccessibleContextNode control = targetControl.GetApiControl<AccessibleContextNode>();
            return control;
        }


        protected void ThrowIfMissingControlEntity()
        {
            if (this.ControlEntity == null)
            {
                throw new ConfigurationException($"Component with id : {this.Id} must be child of WinControlEntity");
            }
        }
    }
}
