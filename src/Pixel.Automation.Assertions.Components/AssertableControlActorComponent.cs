using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Exceptions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Assertions.Components;

/// <summary>
/// Base actor class to assert states of a <see cref="UIControl"/>
/// </summary>
public abstract class AssertableControlActorComponent : ActorComponent
{
    /// <summary>
    /// Target control that needs to be interacted with. This is an optional component. If the actor is an immediate child of a <see cref="ControlEntity"/>,
    /// target control will be automatically retrieved from parent ControlEntity. If the target control was previously looked up by any means , it can be specified as an 
    /// argument instead.
    /// </summary>
    [DataMember]
    [Display(Name = "Target Control", GroupName = "Control Details", Order = 10, Description = "[Optional] Target control that needs to be interacted with")]
    public Argument TargetControl { get; set; } = new InArgument<UIControl>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };


    /// <summary>
    /// Parent control entity that provides the target control.
    /// </summary>
    [Browsable(false)]
    public ControlEntity ControlEntity
    {
        get
        {
            return this.Parent as ControlEntity;

        }
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="tag"></param>
    public AssertableControlActorComponent(string name = "", string tag = "") : base(name, tag)
    {

    }

    /// <summary>
    /// Get the UIControl identified using the control details
    /// </summary>
    /// <returns></returns>
    protected async Task<UIControl> GetTargetControl()
    {
        UIControl targetControl;
        if (this.TargetControl.IsConfigured())
        {
            targetControl = await ArgumentProcessor.GetValueAsync<UIControl>(this.TargetControl);
        }
        else
        {
            ThrowIfMissingControlEntity();
            targetControl = await this.ControlEntity.GetControl();
        }
        return targetControl;
    }

    /// <summary>
    /// Throw <see cref="ConfigurationException"/> if ControlEntity is missing.
    /// </summary>
    protected void ThrowIfMissingControlEntity()
    {
        if (this.ControlEntity == null)
        {
            throw new ConfigurationException($"Component with id : {this.Id} must be child of ControlEntity");
        }
    }

}
