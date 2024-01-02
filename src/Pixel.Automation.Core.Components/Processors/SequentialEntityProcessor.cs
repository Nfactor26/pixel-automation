using Pixel.Automation.Core.Arguments;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Processors;

/// <summary>
/// Sequential Entity Processor are used at design time for Prefab editors allowing execution of the Prefab.    
/// </summary>
[DataContract]
[Serializable]    
public class SequentialEntityProcessor : EntityProcessor
{
    [DataMember]
    [Display(Name = "Post Processing Delay", GroupName = "Delay", Order = 10)]
    [Description("Delay after execution of an actor")]
    public Argument PostDelay { get; set; } = new InArgument<int>() { DefaultValue = 100, CanChangeType = false };

    [DataMember]
    [Display(Name = "Delay Factor", GroupName = "Delay", Order = 20)]
    [Description("Scaling factor for Post delay")]
    public Argument DelayFactor { get; set; } = new InArgument<int>() { DefaultValue = 3, CanChangeType = false };


    /// <summary>
    /// constructor
    /// </summary>
    public SequentialEntityProcessor() : base("Sequential Processor", "Processor")
    {

    }

    /// <inheritdoc/>
    public override async Task BeginProcessAsync()
    {
        ResetComponents();
        ResetDelay();
        await ConfigureDelay();
        await ProcessEntity(this);       
    }

    private async Task ConfigureDelay()
    {
        var argumentProcessor = this.ArgumentProcessor;
        int delayFactor = 1;
        if (this.DelayFactor.IsConfigured())
        {
            delayFactor = await argumentProcessor.GetValueAsync<int>(this.DelayFactor);
        }
        if (this.PostDelay.IsConfigured())
        {
            this.postDelayAmount = delayFactor * await argumentProcessor.GetValueAsync<int>(this.PostDelay);
        }           
       
    }
}
