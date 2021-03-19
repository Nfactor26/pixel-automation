extern alias uiaComWrapper;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Resize", "UIA", iconSource: null, description: "Trigger Transform pattern on AutomationElement to resize", tags: new string[] { "Resize", "UIA" })]

    public class ResizeActorComponent : UIAActorComponent
    {

        [DataMember]
        public Argument SizeX { get; set; } = new InArgument<double>() { CanChangeMode = true, CanChangeType = false, DefaultValue = 0.0, Mode = ArgumentMode.Default };

        [DataMember]
        public Argument SizeY { get; set; } = new InArgument<double>() { CanChangeMode = true, CanChangeType = false, DefaultValue = 0.0, Mode = ArgumentMode.Default };

        public ResizeActorComponent() : base("Resize", "Resize")
        {

        }

        public override void Act()
        {
            AutomationElement control = GetTargetControl();

            double sizeX = this.ArgumentProcessor.GetValue<double>(this.SizeX);
            double sizeY = this.ArgumentProcessor.GetValue<double>(this.SizeY);

            control.ResizeTo(sizeX, sizeY);
        }

    }
}
