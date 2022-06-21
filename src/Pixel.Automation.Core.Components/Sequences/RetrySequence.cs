using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components.Processors;
using Polly;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Components.Sequences
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Retry", "Sequences", iconSource: null, description: "Retry Sequence", tags: new string[] { "Retry", "Sequence" })]
    public class RetrySequence : EntityProcessor
    {
        private readonly ILogger logger = Log.ForContext<RetrySequence>();

        [DataMember]
        [Display(Name = "Count", GroupName = "Retry Configuration", Order = 10)]
        [Description("Number of retry attempts")]
        public Argument RetryCount { get; set; } = new InArgument<int>() { CanChangeType = false, DefaultValue = 5 };

        [DataMember]
        [Display(Name = "Interval (seconds)", GroupName = "Retry Configuration", Order = 20)]
        [Description("Interval between subsequent retries")]
        public Argument RetryInterval { get; set; } = new InArgument<double>() { CanChangeType = false, DefaultValue = 1 };

        [DataMember]
        [Display(Name = "Exception", GroupName = "Retry Configuration", Order = 30)]
        [Description("Last exception encountered during processing of execute block. This can be used inside retry block.")]
        public Argument Exception { get; set; }  = new OutArgument<Exception>() { CanChangeType = false }; 

        public RetrySequence() : base("Retry Sequence", "RetrySequence")
        {

        }

        public override async Task BeginProcessAsync()
        {
            int retryCount = await this.ArgumentProcessor.GetValueAsync<int>(RetryCount);
            double retryInterval = await this.ArgumentProcessor.GetValueAsync<double>(RetryInterval);
            TimeSpan sleepDuration = TimeSpan.FromMilliseconds(retryInterval * 1000);

            var retrySequence = new List<TimeSpan>();
            foreach (var i in Enumerable.Range(1, retryCount))
            {
                retrySequence.Add(sleepDuration);
            }

            var policy = Policy.Handle<Exception>().WaitAndRetryAsync(retrySequence, async (exception, waitInterval, currentAttempt, context) =>
            {
                logger.Warning($"An error was encountered while processing Execute block. {exception.Message}.");
                await this.ArgumentProcessor.SetValueAsync<Exception>(Exception, exception);
                var retry = this.GetComponentsByName("Retry", Enums.SearchScope.Children).FirstOrDefault() as Entity;
                await this.ProcessEntity(retry);
                logger.Information($"Number of remaining attempts is {retryCount - currentAttempt} to process Execute block.");
            });

            await policy.ExecuteAsync(async() =>
            {
                logger.Information($"Processing execute block.");
                var execute = this.GetComponentsByName("Execute", Enums.SearchScope.Children).FirstOrDefault() as Entity;
                await this.ProcessEntity(execute);
            });
        }

        public override IEnumerable<IComponent> GetNextComponentToProcess()
        {
            yield break;
        }

        public override void ResolveDependencies()
        {
            if (this.Components.Count() > 0)
            {
                return;
            }

            PlaceHolderEntity executeBlock = new PlaceHolderEntity("Execute");         
            PlaceHolderEntity retryBlock = new PlaceHolderEntity("Retry");
            base.AddComponent(executeBlock);          
            base.AddComponent(retryBlock);
        }

        public override Entity AddComponent(IComponent component)
        {
            return this;
        }
    }
}
