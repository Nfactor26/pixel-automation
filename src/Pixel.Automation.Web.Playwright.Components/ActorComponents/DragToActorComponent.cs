using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="DragToActorComponent"/> to drag currrent element on to another element.  
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Drag To", "Playwright", iconSource: null, description: "Perform a drag drop operation on an element", tags: new string[] { "drag", "Web" })]
public class DragToActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<DragToActorComponent>();

    /// <summary>
    /// Target <see cref="WebUIControl"/> which is the drop target
    /// </summary>
    [DataMember]
    [Display(Name = "Target", GroupName = "Configuration", Order = 10, Description = "Input argument for drop target locator")]
    public Argument Target { get; set; } = new InArgument<WebUIControl>() {  Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Optional input argument for <see cref="LocatorDragToOptions"/> that can be used to customize the drag operation
    /// </summary>
    [DataMember]
    [Display(Name = "Drag To Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorDragToOptions")]
    public Argument DragToOptions { get; set; } = new InArgument<LocatorDragToOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public DragToActorComponent() : base("Drag To", "DragTo")
    {

    }

    /// <summary>
    /// Drag current control to specified target control
    /// </summary>
    public override async Task ActAsync()
    {
        var targetControl = await this.ArgumentProcessor.GetValueAsync<WebUIControl>(this.Target);
        var dragToOptions = this.DragToOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorDragToOptions>(this.DragToOptions) : null;
        var (name, control) = await GetTargetControl();
        await control.DragToAsync(targetControl.GetApiControl<ILocator>(), dragToOptions);
        logger.Information("Control : '{0}' was dragged to target control : '{1}'", name, targetControl.ControlName);
    }
}
