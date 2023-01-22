using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Get an element's tag name
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Tag Name", "Appium", iconSource: null, description: "Get an element's tag name", tags: new string[] { "name", "tag name" })]
public class GetTagNameActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetTagNameActorComponent>();

    /// <summary>
    /// Argument where the retrieved text for element will be stored
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Retrieved tag name for element")]
    public Argument Result { get; set; } = new OutArgument<string>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetTagNameActorComponent() : base("Get Tag Name", "GetTagName")
    {

    }

    /// <summary>
    /// Retrieve the value of configured TagName from <see cref="IWebElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        IWebElement control = await GetTargetControl();
        string extractedValue = control.TagName;
        await ArgumentProcessor.SetValueAsync<string>(Result, extractedValue);
        logger.Information("Retrived  tag name from control.");
    }

}
