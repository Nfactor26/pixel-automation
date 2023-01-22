using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="GetTextActorComponent"/> to retrieve visible text for element.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Text", "Appium", iconSource: null, description: "Get the visibile text for element", tags: new string[] { "text", "get text"})]
public class GetTextActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetTextActorComponent>();
    
    /// <summary>
    /// Argument where the retrieved text for element will be stored
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Retrieved text for element")]
    public Argument Result { get; set; } = new OutArgument<string>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetTextActorComponent() : base("Get Text", "GetText")
    {

    }

    /// <summary>
    /// Retrieve the value of configured ElementText from <see cref="IWebElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        IWebElement control = await GetTargetControl();       
        string extractedValue = control.Text;
        await ArgumentProcessor.SetValueAsync<string>(Result, extractedValue);
        logger.Information("Retrived  visible text from control.");
    }

}
