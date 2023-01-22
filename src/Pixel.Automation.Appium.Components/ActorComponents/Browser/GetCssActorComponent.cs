using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="GetCssActorComponent"/> to retrieve the value of any css property from a web control.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Css", "Appium", "Browser", iconSource: null, description: "Get the value of css property of a WebElement", tags: new string[] { "css", "get" })]
public class GetCssActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetCssActorComponent>();

    /// <summary>
    /// Name of the attribute whose value needs to be retrieved
    /// </summary>
    [DataMember(IsRequired = true)]
    [Display(Name = "Css Property Name", GroupName = "Configuration", Order = 10, Description = "Name of the css property whose value needs to be retrieved")]
    public Argument CssPropertyName { get; set; } = new InArgument<string>() { DefaultValue = "value", Mode = ArgumentMode.Default };

    /// <summary>
    /// Argument where the value of the attribute will be stored
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Store the result in specified Argument")]       
    public Argument Result { get; set; } = new OutArgument<string>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetCssActorComponent() : base("Get Css", "GetCss")
    {

    }

    /// <summary>
    /// Retrieve the value of configured attribute from <see cref="IWebElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        IWebElement control = await GetTargetControl();
        var cssPropertyName = await this.ArgumentProcessor.GetValueAsync<string>(this.CssPropertyName);
        string extractedValue = control.GetCssValue(cssPropertyName);
        await ArgumentProcessor.SetValueAsync<string>(Result, extractedValue);
        logger.Information("Retrived  value of css : {0} from control.", cssPropertyName);
    }

}
