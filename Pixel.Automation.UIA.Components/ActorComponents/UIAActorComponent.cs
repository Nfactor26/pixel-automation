extern alias uiaComWrapper;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    public abstract class UIAActorComponent : ActorComponent
    {
        [DataMember]
        [DisplayName("Target Control")]
        [Category("Control Details")]           
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };


        [RequiredComponent]
        [Browsable(false)]
        public WinApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetApplicationDetails<WinApplication>(this);
            }
        }

        [Browsable(false)]
        public WinControlEntity ControlEntity
        {
            get
            {
                return this.Parent as WinControlEntity;

            }
        }

        protected UIAActorComponent(string name = "", string tag = "") : base(name, tag)
        {

        }

        protected AutomationElement GetTargetControl()
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

            AutomationElement control = targetControl.GetApiControl<AutomationElement>();
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
