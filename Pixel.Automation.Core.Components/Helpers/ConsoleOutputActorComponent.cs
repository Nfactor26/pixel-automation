using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.Helpers
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Write To Console", "Utility", iconSource: null, description: "Print a message to console", tags: new string[] { "Console", "Utility" })]
    public class ConsoleOutputActorComponent : ActorComponent
    {
        [DataMember]        
        [Description("Message to be written to console")]
        [Display(Name = "Message", GroupName = "Input", Order = 10)]
        public Argument Message { get; set; } = new InArgument<string>() { CanChangeType = true };

        public ConsoleOutputActorComponent() : base("Write To Console", "ConsoleOutput")
        {

        }

        public override void Act()
        {
            var argumentProcessor = this.ArgumentProcessor;       
            string message = this.Message.GetValue(argumentProcessor)?.ToString() ?? string.Empty;
            Console.WriteLine(message);
        }
    }
}
