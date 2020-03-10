using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Scroll", "UIA", iconSource: null, description: "Perform horizontal/vertical scroll", tags: new string[] { "Scroll", "UIA" })]

    public class ScrollActorComponent : UIAActorComponent
    {

        [DataMember]
        public Argument HorizontalScrollAmount { get; set; } = new InArgument<ScrollAmount>() { CanChangeMode = true, CanChangeType = false, DefaultValue = 0.0, Mode = ArgumentMode.Default };

        [DataMember]
        public Argument VerticalScrollAmount { get; set; } = new InArgument<ScrollAmount>() { CanChangeMode = true, CanChangeType = false, DefaultValue = 0.0, Mode = ArgumentMode.Default };

        public ScrollActorComponent() : base("Scroll", "Scroll")
        {

        }

        public override void Act()
        {
            AutomationElement control = GetTargetControl();

            ScrollAmount horizontalScrollAmount = this.ArgumentProcessor.GetValue<ScrollAmount>(this.HorizontalScrollAmount);
            ScrollAmount verticalScrollAmount = this.ArgumentProcessor.GetValue<ScrollAmount>(this.VerticalScrollAmount);

            control.Scroll(horizontalScrollAmount, verticalScrollAmount);
        }

    }
}
