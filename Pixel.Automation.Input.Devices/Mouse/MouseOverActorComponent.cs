using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Input.Devices
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Mouse Over", "Input Device", "Mouse", iconSource: null, description: "Move mouse to desired coordinates", tags: new string[] { "Click" })]

    public class MouseOverActorComponent : DeviceInputActor
    {
        [DataMember]
        [Display(Name = "Target Control", GroupName = "Control Details")]     
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        [DataMember]
        [Display(Name = "Move To", GroupName = "Mouse Over configuration", AutoGenerateField = false)]
        [Description("Represents the coordinates to which cursor is moved.This is auto calculated if Control and CoordinateProvider are configured.")]            
        public Argument MoveTo { get; set; } = new InArgument<ScreenCoordinate>()
        {
            DefaultValue = new ScreenCoordinate(),
            CanChangeType = false
        };


        Target target = Target.Control;
        [DataMember]
        [Display(Name = "Target", GroupName = "Mouse Over Configuration")]
        [Description("Configure if mouse target is a control  or specified coordinates")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public Target Target
        {
            get => target;
            set
            {
                switch (value)
                {
                    case Target.Control:
                        this.SetDispalyAttribute(nameof(MoveTo), false);
                        break;
                    case Target.Empty:
                        this.SetDispalyAttribute(nameof(MoveTo), true);
                        break;
                }
                MoveTo.Mode = ArgumentMode.Default;
                target = value;
            }
        }

        [DataMember]
        [Display(Name = "Smooth Mode", GroupName = "Mouse Over Configuration")]
        [Description("Controls how the mouse moves between two points")]      
        public SmoothMode SmootMode { get; set; } = SmoothMode.Interpolated;


        public MouseOverActorComponent() : base("Mouse Over", "MouseOver")
        {

        }

        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;

            ScreenCoordinate screenCoordinate = default;
            switch (this.Target)
            {
                case Target.Control:
                    screenCoordinate = GetScreenCoordinateFromControl(this.TargetControl as InArgument<UIControl>);
                    break;
                case Target.Empty:
                    screenCoordinate = argumentProcessor.GetValue<ScreenCoordinate>(this.MoveTo);
                    break;
            }
            var syntheticMouse = GetMouse();        
            syntheticMouse.MoveMouseTo(screenCoordinate, this.SmootMode);
             


        }

        public override bool ValidateComponent()
        {
            if (!this.TargetControl.IsConfigured() && this.ControlEntity == null)
            {
                IsValid = false;
            }
            return IsValid && base.ValidateComponent();
        }
    }
}
