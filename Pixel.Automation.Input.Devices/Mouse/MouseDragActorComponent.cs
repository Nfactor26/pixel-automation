﻿using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Input.Devices;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;

namespace Nish26.Automation.Input.Devices
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Drag", "Input Device", "Mouse", iconSource: null, description: "Perform a drag action using mouse", tags: new string[] { "Drag" })]

    public class MouseDragActorComponent : InputSimulatorBase
    {
        [DataMember]
        [DisplayName("Target Control")]
        [Category("Control Details")]             
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };


        [DataMember]      
        [DisplayName("Drag Point")]
        [Description("Represents the coordinates at which drag starts/ends. This is auto calculated if Control and CoordinateProvider are configured.")]
        [Category("Drag Congfiguration")]          
        [Browsable(true)]
        public Argument DragPoint { get; set; } = new InArgument<ScreenCoordinate>()
        {
            DefaultValue = new ScreenCoordinate(),
            CanChangeType = false
        };


        DragMode dragMode;
        [DataMember]
        [DisplayName("Drag Mode")]
        [Category("Drag Congfiguration")]       
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
                            this.Name = this.Name.Replace(" [End]", "");

                        if (!this.Name.EndsWith(" [Start]"))
                            this.Name += " [Start]";

                        break;
                    case DragMode.To:
                        if (this.Name.EndsWith(" [Start]"))
                            this.Name = this.Name.Replace(" [Start]", "");

                        if (!this.Name.EndsWith(" [End]"))
                            this.Name += " [End]";
                        break;
                }
                OnPropertyChanged("Name");

            }

        }

        Target target = Target.Control;
        [DataMember]
        [DisplayName("Target Control")]
        [Description("Configure if mouse target is a control  or specified coordinates")]
        [Category("Click Configuration")]   
        public Target Target
        {
            get => target;
            set
            {
                switch (value)
                {
                    case Target.Control:
                        this.SetBrowsableAttribute(nameof(DragPoint), false);
                        break;
                    case Target.Empty:
                        this.SetBrowsableAttribute(nameof(DragPoint), true);
                        break;
                }
                DragPoint.Mode = ArgumentMode.Default;
                target = value;
            }
        }

        [DataMember]
        [DisplayName("Smooth Mode")]
        [Description("Controls how the mouse moves between two points")]
        [Category("Click Configuration")]       
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
                    UIControl targetControl = default;
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
            if (!this.TargetControl.IsConfigured() && this.ControlEntity == null)
            {
                IsValid = false;
            }
            return IsValid && base.ValidateComponent();
        }
    }
}
