using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Input.Devices
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Type text", "Input Device", "Keyboard", iconSource: null, description: "Type text using keyboard", tags: new string[] { "Type" })]

    public class TypeTextActorComponent : InputSimulatorBase
    {
        [DataMember]
        [Category("Input Text Configuration")]    
        public Argument Input { get; set; } = new InArgument<string>();

        public TypeTextActorComponent() : base("Type text", "TypeText")
        {

        }

        public override void Act()
        {           
            string textToType = this.ArgumentProcessor.GetValue<string>(this.Input);

            if (!string.IsNullOrEmpty(textToType))
            {
                var keyboard = GetKeyboard();
                keyboard.TypeText(textToType);
            }

        }

    }
}
