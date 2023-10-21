using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Base class for Selenium Actors
/// </summary>
[DataContract]
[Serializable]
public abstract class SeleniumActorComponent : ActorComponent
{
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
    /// Constructor
    /// </summary>
    /// <param name="name">Name of the component</param>
    /// <param name="tag">Tag for the component</param>
    protected SeleniumActorComponent(string name = "", string tag = "") : base(name, tag)
    {

    }

    /// <summary>
    /// Take a screen shot if capturing screenshot is enabled after Act method finishes
    /// </summary>
    /// <returns></returns>
    public override async Task OnCompletionAsync()
    {
        if (TraceManager.IsEnabled)
        {
            await CaptureScreenShotAsync();
        }
    }

    /// <summary>
    /// Capture screenshot of the active page
    /// </summary>
    /// <returns></returns>
    public async Task CaptureScreenShotAsync()
    {
        var ownerApplicationEntity = this.EntityManager.GetApplicationEntity(this);
        if(TraceManager.IsEnabled && ownerApplicationEntity.AllowCaptureScreenshot)
        {
            string imageFile = Path.Combine(this.EntityManager.GetCurrentFileSystem().TempDirectory, $"{Path.GetRandomFileName()}.png");
            await ownerApplicationEntity.CaptureScreenShotAsync(imageFile);
            TraceManager.AddImage(Path.GetFileName(imageFile));
        }       
    }
}

/// <summary>
/// Base class for Selenium actors that operated on a web control.
/// </summary>
public abstract class WebElementActorComponent : SeleniumActorComponent
{
    /// <summary>
    /// If the control to be interacted has been already looked up , it can be specified as an argument.
    /// TargetControl Argument takes precedence over parent Control Entity.
    /// </summary>
    [DataMember]
    [Display(Name = "Target Control", GroupName = "Control Details", Order = 10, Description = "[Optional] Specify a WebUIControl to be interacted with.")]       
    public Argument TargetControl { get; set; } = new InArgument<UIControl>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound, CanChangeType = false };

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
    protected WebElementActorComponent(string name = "", string tag = "") : base(name, tag)
    {

    }

    /// <summary>
    /// Retrieve the target control specified either as an <see cref="Argument"/> or a parent <see cref="WebControlEntity"/>
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<(string, IWebElement)> GetTargetControl()
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

        return (targetControl.ControlName, targetControl.GetApiControl<IWebElement>());
    }


    protected void ThrowIfMissingControlEntity()
    {
        if (this.ControlEntity == null)
        {
            throw new ConfigurationException($"Component with id : {this.Id} must be child of WebControlEntity");
        }
    }
}
