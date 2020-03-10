using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Get Value", "UIA", iconSource: null, description: "Trigger Value pattern on AutomationElement to GetValue", tags: new string[] { "GetValue", "UIA" })]

    public class GetValueActorComponent : UIAActorComponent
    {       
        [DataMember]      
        public Argument Output { get; set; } = new OutArgument<string>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        public GetValueActorComponent() : base("Get Value","GetValue")
        {

        }

        public override void Act()
        {
            AutomationElement control = GetTargetControl();

            string result = control.GetValue();
            ArgumentProcessor.SetValue<string>(Output, result);
        }

    }
}
