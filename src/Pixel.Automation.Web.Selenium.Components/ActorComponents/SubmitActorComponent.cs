using OpenQA.Selenium;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="SubmitActorComponent"/> to perform submit on a <see cref="IWebElement"/>
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Submit", "Selenium", iconSource: null, description: "Perform a click action on WebElement", tags: new string[] { "Click", "Web" })]
public class SubmitActorComponent : WebElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<SubmitActorComponent>();       
   

    public SubmitActorComponent() : base("Submit", "Submit")
    {

    }

    /// <summary>
    /// Perform submit on <see cref="IWebElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        var (name,control) = await GetTargetControl();
        control.Submit();
        logger.Information("Submit executed on control : '{0}' ", name);
    }
}
