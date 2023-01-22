using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Set the context being automated
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Set Context", "Appium", "Session", iconSource: null, description: "Set the context being automated", tags: new string[] { "set", "context" })]
public class SetContextActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<SetContextActorComponent>();

    [DataMember]
    [Display(Name = "Context", GroupName = "Input", Order = 10, Description = "Context to automate")]
    public Argument Context { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = "NATIVE_APP" };
     

    /// <summary>
    /// Default constructor
    /// </summary>
    public SetContextActorComponent() : base("Set Context", "SetContext")
    {

    }

    /// <summary>
    /// Set the context being automated
    /// </summary>
    /// <returns></returns>
    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var context = await this.ArgumentProcessor.GetValueAsync<string>(this.Context);
        driver.Context = context;
        logger.Information("Context was switched to {0}", context);
    }
}
