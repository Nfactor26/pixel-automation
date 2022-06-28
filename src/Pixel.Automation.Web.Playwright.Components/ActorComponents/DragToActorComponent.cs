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
    /// Target <see cref="ILocator"/> which is the drop target
    /// </summary>
    [DataMember]
    [Display(Name = "Target", GroupName = "Configuration", Order = 10, Description = "Input argument for drop target locator")]
    public Argument Target { get; set; } = new InArgument<ILocator>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Optional input argument for <see cref="LocatorDragToOptions"/> that can be used to customize the drag operation
    /// </summary>
    [DataMember]
    [Display(Name = "Drag To Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorDragToOptions")]
    public Argument DragToOptions { get; set; } = new InArgument<LocatorDragToOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public DragToActorComponent() : base("DragTo", "DragTo")
    {

    }

    /// <summary>
    /// Drag current control to specified target control
    /// </summary>
    public override async Task ActAsync()
    {
        var targetLocator = await this.ArgumentProcessor.GetValueAsync<ILocator>(this.Target);
        var dragToOptions = this.DragToOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorDragToOptions>(this.DragToOptions) : null;
        var control = await GetTargetControl();
        await control.DragToAsync(targetLocator, dragToOptions);
        logger.Information("control was dragged to target.");
    }

    public override string ToString()
    {
        return "DragTo Actor";
    }
}
