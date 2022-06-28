using Microsoft.Playwright;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

public abstract class PlaywrightActorComponent : ActorComponent
{
    /// <summary>
    /// If the control to be interacted has been already looked up , it can be specified as an argument.
    /// TargetControl Argument takes precedence over parent Control Entity.
    /// </summary>
    [DataMember]
    [Display(Name = "Target Control", GroupName = "Control Details", Order = 10, Description = "[Optional] Input argument to provide a WebUIControl.")]
    public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

    /// <summary>
    /// Owner application that is interacted with
    /// </summary>      
    [Browsable(false)]
    public WebApplication ApplicationDetails
    {
        get
        {
            return this.EntityManager.GetOwnerApplication<WebApplication>(this);
        }
    }    

    /// <summary>
    /// Parent Control entity component if any
    /// </summary>
    [Browsable(false)]
    public IControlEntity ControlEntity
    {
        get
        {
            return this.Parent as IControlEntity;

        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">Name of the component</param>
    /// <param name="tag">Tag for the component</param>
    protected PlaywrightActorComponent(string name = "", string tag = "") : base(name, tag)
    {

    }


    /// <summary>
    /// Retrieve the target control specified either as an <see cref="Argument"/> or a parent <see cref="WebControlEntity"/>
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<ILocator> GetTargetControl()
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

        return targetControl.GetApiControl<ILocator>();
    }


    protected void ThrowIfMissingControlEntity()
    {
        if (this.ControlEntity == null)
        {
            throw new ConfigurationException($"Component with id : {this.Id} must be child of WebControlEntity");
        }
    }

}
