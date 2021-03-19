using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.Serialization;

namespace Pixel.Automation.Input.Devices
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Move", "Input Device", "Mouse", iconSource: null, description: "Move mouse by configured coordinates", tags: new string[] { "Move By" })]

    public class MouseMoveByActorComponent : DeviceInputActor
    {
        [DataMember]      
        [Display(Name = "Move By", GroupName = "Mouse Move Configuration")]
        [Description("Represents the amount by which cursor should be moved")]       
        public Argument MoveBy { get; set; } = new InArgument<Point>()
        {
            DefaultValue = new Point(),
            CanChangeType = false
        };

        [DataMember]
        [Display(Name = "Smooth Mode", GroupName = "Mouse Move Configuration")]
        [Description("Controls how the mouse moves between two points")]      
        public SmoothMode SmootMode { get; set; } = SmoothMode.Interpolated;

        public MouseMoveByActorComponent() : base("Move By", "Move By")
        {
        }

        public override void Act()
        {           
            var offsetCoordinates = this.ArgumentProcessor.GetValue<Point>(this.MoveBy);
            var syntheticMouse = GetMouse();
            syntheticMouse.MoveMouseBy(offsetCoordinates.X, offsetCoordinates.Y, this.SmootMode);       
        }
    }
}
