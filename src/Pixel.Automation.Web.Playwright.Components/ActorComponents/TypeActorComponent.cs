using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="TypeActorComponent"/> to type in to text field character by character.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Type", "Playwright", iconSource: null, description: "Type in to text field character by character", tags: new string[] { "Type", "Web" })]

public class TypeActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<TypeActorComponent>();

    /// <summary>
    /// Input argument to provide the text to be typed
    /// </summary>
    [DataMember(IsRequired = true)]
    [Display(Name = "Input", GroupName = "Configuration", Order = 10, Description = "Input argument for text to type")]
    public Argument Input { get; set; } = new InArgument<string>() { CanChangeType = false, Mode = ArgumentMode.DataBound };


    /// <summary>
    ///  Optional input argument for <see cref="LocatorTypeOptions"/> that can be used to customize the type oepration
    /// </summary>
    [DataMember]
    [Display(Name = "Type Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorTypeOptions")]
    public Argument TypeOptions { get; set; } = new InArgument<LocatorTypeOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };


    /// <summary>
    /// Default constructor
    /// </summary>
    public TypeActorComponent() : base("Type", "Type")
    {

    }

    /// <summary>
    /// Type in to text field character by character using TypeAsync() method
    /// </summary>
    public override async Task ActAsync()
    {
        var input = await this.ArgumentProcessor.GetValueAsync<string>(this.Input);
        var control = await GetTargetControl();
        await control.TypeAsync(input, this.TypeOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorTypeOptions>(this.TypeOptions) : null);
        logger.Information("Type performed on element.");
    }

    ///</inheritdoc>
    public override string ToString()
    {
        return "Type Actor";
    }
}
