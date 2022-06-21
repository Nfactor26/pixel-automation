using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Waits
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Delay (seconds)", "Core Components", iconSource: null, description: "Wait for a specified amout of time in seconds", tags: new string[] { "Wait", "Core" })]
    public class DelayActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<DelayActorComponent>();

        [DataMember]
        [Description("Amount of time in seconds to wait e.g. 0.5 , 1 ,10 ,etc.")]
        [Display(Name = "Delay Amount (seconds)", Order = 10, GroupName = "Input")]
        public Argument WaitAmount { get; set; } = new InArgument<double>() { DefaultValue = 5, CanChangeType = false};

        public DelayActorComponent() : base("Delay", "DelayActorComponent")
        {

        }

        public override async Task ActAsync()
        {
            var argumentProcessor = this.ArgumentProcessor;
            double waitAmount = await argumentProcessor.GetValueAsync<double>(this.WaitAmount);
            logger.Information($"Sleep for {waitAmount} seconds.");
            Thread.Sleep((int)(waitAmount * 1000));
            await Task.CompletedTask;
        }
    }
}
