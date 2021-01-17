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
using System.Threading;

namespace Pixel.Automation.Input.Devices
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Drag", "Input Device", "Mouse", iconSource: null, description: "Perform a drag action using mouse", tags: new string[] { "Drag" })]

    public class MouseDragActorComponent : DeviceInputActor
    {
        [DataMember]
        [Display(Name = "Target Control", GroupName = "Control Details")]  
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };


        [DataMember]
        [Display(Name = "Drag Point", GroupName = "Drag Configuration", AutoGenerateField = false)]
        [Description("Represents the coordinates at which drag starts/ends. This is auto calculated if Control and CoordinateProvider are configured.")]       
        public Argument DragPoint { get; set; } = new InArgument<ScreenCoordinate>()
        {
            DefaultValue = new ScreenCoordinate(),
            CanChangeType = false
        };


        DragMode dragMode;
        [DataMember]
        [Display(Name = "Drag Mode", GroupName = "Drag Configuration")]     
        public DragMode DragMode
        {
            get => this.dragMode;
            set
            {
                this.dragMode = value;
                switch (value)
                {
                    case DragMode.From:

                        if (this.Name.EndsWith(" [End]"))
                        {
                            this.Name = this.Name.Replace(" [End]", "");
                        }

                        if (!this.Name.EndsWith(" [Start]"))
                        {
                            this.Name += " [Start]";
                        }

                        break;
                    case DragMode.To:
                        if (this.Name.EndsWith(" [Start]"))
                        {
                            this.Name = this.Name.Replace(" [Start]", "");
                        }

                        if (!this.Name.EndsWith(" [End]"))
                        {
                            this.Name += " [End]";
                        }
                        break;
                }
                OnPropertyChanged(nameof(Name));
            }

        }

        Target target = Target.Control;
        [DataMember]
        [Display(Name = "Target", GroupName = "Drag Configuration")]
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
                        this.SetDispalyAttribute(nameof(DragPoint), false);
                        break;
                    case Target.Empty:
                        this.SetDispalyAttribute(nameof(DragPoint), true);
                        break;
                }
                DragPoint.Mode = ArgumentMode.Default;
                target = value;
            }
        }

        [DataMember]
        [Display(Name = "Smooth Mode", GroupName = "Drag Configuration")]
        [Description("Controls how the mouse moves between two points")]           
        public SmoothMode SmootMode { get; set; } = SmoothMode.Interpolated;

        public MouseDragActorComponent() : base("Drag", "MouseDrag")
        {
            this.DragMode = DragMode.From;
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
                    screenCoordinate = argumentProcessor.GetValue<ScreenCoordinate>(this.DragPoint);
                    break;
            }

            var syntheticMouse = GetMouse();
            switch (this.dragMode)
            {
                case DragMode.From:
                    syntheticMouse.MoveMouseTo(screenCoordinate, this.SmootMode);
                    Thread.Sleep(500);
                    syntheticMouse.ButtonDown(MouseButton.LeftButton);
                    break;
                case DragMode.To:
                    syntheticMouse.MoveMouseTo(screenCoordinate, this.SmootMode);
                    Thread.Sleep(500);
                    syntheticMouse.ButtonUp(MouseButton.LeftButton);
                    break;
            }

        }

        public override bool ValidateComponent()
        {
            switch (this.Target)
            {
                case Target.Empty:
                    IsValid = this.DragPoint?.IsConfigured() ?? false;
                    break;
                case Target.Control:
                    IsValid = (this.TargetControl?.IsConfigured() ?? false) || this.ControlEntity != null;
                    break;
            }
            return IsValid && base.ValidateComponent();
        }
    }
}
