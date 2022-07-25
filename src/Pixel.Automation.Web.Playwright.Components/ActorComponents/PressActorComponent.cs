using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="PressActorComponent"/> to press keys and shortcut.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Press", "Playwright", iconSource: null, description: "Press keys and shortcuts", tags: new string[] { "press", "Web" })]

public class PressActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<PressActorComponent>();

    /// <summary>
    /// Input argument used to provide keys to press
    /// </summary>
    [DataMember(IsRequired = true)]
    [Display(Name = "Keys", GroupName = "Configuration", Order = 10, Description = "Input argument to provide keys to press")]
    public Argument Keys { get; set; } = new InArgument<string>() { Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Optional input argument for <see cref="LocatorPressOptions"/> that can be used to customize the Press operation
    /// </summary>
    [DataMember]
    [Display(Name = "Press Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorpressOptions")]
    public Argument PressOptions { get; set; } = new InArgument<LocatorPressOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public PressActorComponent() : base("Press", "Press")
    {

    }

    /// <summary>
    /// Press specified keys and shortcuts using PressAsync() method
    /// </summary>
    public override async Task ActAsync()
    {
        var input = await this.ArgumentProcessor.GetValueAsync<string>(this.Keys);
        var control = await GetTargetControl();
        await control.PressAsync(input, this.PressOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorPressOptions>(this.PressOptions) : null);
        logger.Information("Key press performed on element.");
    }

    ///</inheritdoc>
    public override string ToString()
    {
        return "Press Actor";
    }
}
