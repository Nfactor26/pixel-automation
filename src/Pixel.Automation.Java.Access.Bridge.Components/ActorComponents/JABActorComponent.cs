using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    [DataContract]
    [Serializable]
    public abstract class JABActorComponent : ActorComponent
    {

        [DataMember]
        [DisplayName("Target Control")]
        [Category("Control Details")]
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };
              

        [Browsable(false)]
        public JavaControlEntity ControlEntity
        {
            get
            {
                return this.Parent as JavaControlEntity;

            }
        }
        protected JABActorComponent(string name = "", string tag = "") : base(name, tag)
        {

        }


        protected AccessibleContextNode GetTargetControl()
        {
            UIControl targetControl;
            if (this.TargetControl.IsConfigured())
            {
                targetControl = ArgumentProcessor.GetValue<UIControl>(this.TargetControl);
            }
            else
            {
                ThrowIfMissingControlEntity();
                targetControl = this.ControlEntity.GetControl();
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
