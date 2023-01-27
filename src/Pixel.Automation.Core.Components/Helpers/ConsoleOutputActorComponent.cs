using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Helpers
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Write To Console", "Utility", iconSource: null, description: "Print a message to console", tags: new string[] { "Console", "Utility" })]
    public class ConsoleOutputActorComponent : ActorComponent
    {
        [DataMember(Order = 200)]
        [Display(Name = "Message", GroupName = "Input", Order = 10, Description = "Message to be written to console")]
        public Argument Message { get; set; } = new InArgument<string>() { CanChangeType = true };

        public ConsoleOutputActorComponent() : base("Write To Console", "ConsoleOutput")
        {

        }

        public override async Task ActAsync()
        {      
            string message = await this.ArgumentProcessor.GetValueAsync<string>(this.Message);
            Console.WriteLine(message);
            await Task.CompletedTask;
        }
    }
}
