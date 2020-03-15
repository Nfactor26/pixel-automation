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

using MouseButton = Pixel.Automation.Core.Devices.MouseButton;

namespace Pixel.Automation.Input.Devices
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Click", "Input Device", "Mouse", iconSource: null, description: "Perform a click action using mouse at given coordinates", tags: new string[] { "Click" })]

    public class MouseClickActorComponent : InputSimulatorBase
    {
        [DataMember]
        [Display(Name = "Target Control", GroupName = "Control Details")]               
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        [DataMember]       
        [Description("Represents the coordinates at which click is performed.This is auto calculated if Control and CoordinateProvider are configured.")]
        [Display(Name = "Click At", GroupName = "Click Configuration", AutoGenerateField = false)]       
        public Argument ClickAt { get; set; } = new InArgument<ScreenCoordinate>()
        {
             DefaultValue = new ScreenCoordinate(),
             CanChangeType = false
        };

        [DataMember]
        [Display(Name = "Mouse Button", GroupName = "Click Configuration")]
        [Description("Represents the mouse button to click i.e Left/Right/Middle button of the mouse")]   
        public MouseButton Button { get; set; }

        [DataMember]
        [Display(Name= "Click Mode", GroupName = "Click Configuration")]
        [Description("Represents whether Single click or double click will be performed")]           
        public ClickMode ClickMode { get; set; } = ClickMode.SingleClick;

        Target target = Target.Control;
        [DataMember]
        [Display(Name = "Target Control", GroupName = "Click Configuration")]
        [Description("Configure if mouse target is a control  or specified coordinates")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public Target Target
        {
            get => target;
            set
            {
                switch(value)
                {
                    case Target.Control:
                        this.SetDispalyAttribute(nameof(ClickAt), false);                      
                        break;
                    case Target.Empty:
                        this.SetDispalyAttribute(nameof(ClickAt), true);
                        break;
                }
                ClickAt.Mode = ArgumentMode.Default;
                target = value;
            }
        }

        [DataMember]
        [Display(Name = "Smooth Mode", GroupName = "Click Configuration")]
        [Description("Controls how the mouse moves between two points")]    
        public SmoothMode SmootMode { get; set; } = SmoothMode.Interpolated;

        public MouseClickActorComponent() : base("Mouse Click", "MouseClick")
        {
         
        }

        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;

            ScreenCoordinate screenCoordinate = default;
            switch (this.Target)
            {
                case Target.Control:
                    UIControl targetControl;
                    if (this.TargetControl.IsConfigured())
                    {
                        targetControl = argumentProcessor.GetValue<UIControl>(this.TargetControl);
                    }
                    else
                    {
                        ThrowIfMissingControlEntity();
                        targetControl = this.ControlEntity.GetControl();
                    }
                    if (targetControl != null)
                    {                       
                        targetControl.GetClickablePoint(out double x, out double y);
                        screenCoordinate = new ScreenCoordinate(x, y);
                    }
                    break;
                case Target.Empty:
                    screenCoordinate = argumentProcessor.GetValue<ScreenCoordinate>(this.ClickAt);
                    break;
            }

            var syntheticMouse = GetMouse();

        
            syntheticMouse.MoveMouseTo(screenCoordinate, this.SmootMode);
            
           
            switch (Button)
            {

                case MouseButton.LeftButton:
                    switch (this.ClickMode)
                    {
                        case ClickMode.SingleClick:
                            syntheticMouse.Click(MouseButton.LeftButton);
                            break;
                        case ClickMode.DoubleClick:
                            syntheticMouse.DoubleClick(MouseButton.LeftButton);
                            break;
                    }
                    break;
                case MouseButton.MiddleButton:
                    switch (this.ClickMode)
                    {
                        case ClickMode.SingleClick:
                            syntheticMouse.Click(MouseButton.MiddleButton);
                            break;
                        case ClickMode.DoubleClick:
                            syntheticMouse.DoubleClick(MouseButton.MiddleButton);
                            break;
                    }
                    break;
                case MouseButton.RightButton:
                    switch (this.ClickMode)
                    {
                        case ClickMode.SingleClick:
                            syntheticMouse.Click(MouseButton.RightButton);
                            break;
                        case ClickMode.DoubleClick:
                            syntheticMouse.DoubleClick(MouseButton.RightButton);
                            break;
                    }
                    break;
            }
        }

        public override bool ValidateComponent()
        {          
            if(!this.TargetControl.IsConfigured() && this.ControlEntity == null)
            {
                IsValid = false;
            }
            return IsValid && base.ValidateComponent();            
        }
    }   
}
