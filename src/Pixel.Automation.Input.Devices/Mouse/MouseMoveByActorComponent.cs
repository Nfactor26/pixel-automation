﻿using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.Serialization;

namespace Pixel.Automation.Input.Devices
{
    /// <summary>
    /// Use <see cref="MouseMoveByActorComponent"/> to move  cursor by specified amount from it's current position
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Move By", "Input Device", "Mouse", iconSource: null, description: "Move mouse by configured coordinates", tags: new string[] { "Move By" })]
    public class MouseMoveByActorComponent : InputDeviceActor
    {
        private readonly ILogger logger = Log.ForContext<MouseMoveByActorComponent>();

        /// <summary>
        /// Controls how the mouse moves between two points
        /// </summary>
        [DataMember]
        [Display(Name = "Smooth Mode", GroupName = "Configuration", Order = 10, Description = "Controls how the mouse moves between two points")]
        public SmoothMode SmootMode { get; set; } = SmoothMode.Interpolated;

        /// <summary>
        /// Amount in horizontal and vertical direction to move mouse cursor by from it's current position.
        /// </summary>
        [DataMember]      
        [Display(Name = "Move By", GroupName = "Configuration", Order = 20, Description = "Represents the amount by which cursor should be moved from it's current position")]     
        public Argument MoveBy { get; set; } = new InArgument<Point>() { DefaultValue = new Point(), CanChangeType = false };

      
        /// <summary>
        /// Default constructor
        /// </summary>
        public MouseMoveByActorComponent() : base("Move By", "MoveBy")
        {
        }

        /// <summary>
        /// Mouse mouse cursor from it's current position by specified amount.
        /// </summary>
        public override void Act()
        {           
            var offsetCoordinates = this.ArgumentProcessor.GetValue<Point>(this.MoveBy);
            var syntheticMouse = GetMouse();
            syntheticMouse.MoveMouseBy(offsetCoordinates.X, offsetCoordinates.Y, this.SmootMode);
            logger.Information($"Mouse cursor moved by ({offsetCoordinates.X}, {offsetCoordinates.Y})");
        }

        public override string ToString()
        {
            return "Mouse Move By Actor";
        }
    }
}