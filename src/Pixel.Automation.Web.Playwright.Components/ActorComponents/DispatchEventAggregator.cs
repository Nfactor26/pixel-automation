using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="DispatchEventActorComponent"/> to dispatch an event on an element.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Dispatch event", "Playwright", iconSource: null, description: "Dispatch event on an element", tags: new string[] { "dispatch", "event", "Web" })]

public class DispatchEventActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<DispatchEventActorComponent>();

    /// <summary>
    /// Optional LocatorDblClickOptions that can be used to customize double click.
    /// </summary>
    [DataMember]
    [Display(Name = "DispatchEvent", GroupName = "Configuration", Order = 10, Description = "event to dispatch")]
    public Argument DispatchEvent { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = String.Empty };


    /// <summary>
    /// Optional input argument for <see cref="LocatorDispatchEventOptions"/> that can be used to customize how the event is dispatched
    /// </summary>
    [DataMember]
    [Display(Name = "Dispatch Event Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorDispatchEventOptions")]
    public Argument DispatchEventOptions { get; set; } = new InArgument<LocatorDispatchEventOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public DispatchEventActorComponent() : base("Dispatch Event", "DispatchEvent")
    {

    }

    /// <summary>
    /// Dispath a named event on an element using DispatchEventAsync() method
    /// </summary>
    public override async Task ActAsync()
    {
        string dispatchEvent = await this.ArgumentProcessor.GetValueAsync<string>(this.DispatchEvent);
        var dispatchEventOptions = this.DispatchEventOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorDispatchEventOptions>(this.DispatchEventOptions) : null;
        var control = await GetTargetControl();
        await control.DispatchEventAsync(dispatchEvent, dispatchEventOptions);
        logger.Information("Event dispatched on lement.");
    }

}
