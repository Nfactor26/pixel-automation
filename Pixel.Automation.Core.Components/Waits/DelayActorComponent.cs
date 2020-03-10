using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading;

namespace Pixel.Automation.Core.Components.Waits
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Delay (seconds)", "Core Components", iconSource: null, description: "Wait for a specified amout of time in seconds", tags: new string[] { "Wait", "Core" })]
    public class DelayActorComponent : ActorComponent
    {
        [DataMember]
        [Description("Amount of time in seconds to wait e.g. 0.5 , 1 ,10 ,etc.")]
        [Display(Name = "Delay Amount (seconds)", Order = 10, GroupName = "Input")]
        public Argument WaitAmount { get; set; } = new InArgument<double>() { DefaultValue = 5, CanChangeType = false};

        public DelayActorComponent() : base("Delay", "DelayActorComponent")
        {

        }

        public override void Act()
        {
            var argumentProcessor = this.EntityManager.GetServiceOfType<IArgumentProcessor>();
            double waitAmount = argumentProcessor.GetValue<double>(this.WaitAmount);
            Thread.Sleep((int)(waitAmount * 1000));
        }
    }
}
