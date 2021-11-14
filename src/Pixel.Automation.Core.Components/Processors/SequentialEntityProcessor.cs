using Pixel.Automation.Core.Arguments;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Processors
{
    /// <summary>
    /// Sequential Entity Processor are used at design time for Prefab editors allowing execution of the Prefab.    
    /// </summary>
    [DataContract]
    [Serializable]    
    public class SequentialEntityProcessor : EntityProcessor
    {
        [DataMember]
        [Display(Name = "Pre Processing Delay", GroupName = "Delay", Order = 10)]
        [Description("Delay before execution of an actor")]
        public Argument PreDelay { get; set; } = new InArgument<int>() { DefaultValue = 300, CanChangeType = false };

        [DataMember]
        [Display(Name = "Post Processing Delay", GroupName = "Delay", Order = 20)]
        [Description("Delay after execution of an actor")]
        public Argument PostDelay { get; set; } = new InArgument<int>() { DefaultValue = 300, CanChangeType = false };

        /// <summary>
        /// constructor
        /// </summary>
        public SequentialEntityProcessor() : base("Sequential Processor", "Processor")
        {

        }

        /// <inheritdoc/>
        public override async Task BeginProcessAsync()
        {
            ConfigureDelay();
            await ProcessEntity(this);
            ResetDelay();
        }

        private void ConfigureDelay()
        {
            var argumentProcessor = this.ArgumentProcessor;
            if(this.PreDelay.IsConfigured())
            {
                this.preDelayAmount = argumentProcessor.GetValue<int>(this.PreDelay);
            }
            if (this.PostDelay.IsConfigured())
            {
                this.postDelayAmount = argumentProcessor.GetValue<int>(this.PostDelay);
            }           
        }
    }
}
