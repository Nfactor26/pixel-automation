using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="ContextClickActorComponent"/> to perform a context click on web control using selenium webdriver. 
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Context Click", "Selenium", iconSource: null, description: "Perform a context click action on target control", tags: new string[] { "Context Click", "Web" })]
public class ContextClickActorComponent : WebElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<ContextClickActorComponent>();
 
    /// <summary>
    /// Default constructor
    /// </summary>
    public ContextClickActorComponent() : base("Context Click", "ContextClick")
    {

    }

    /// <summary>
    /// Perform a context click on a <see cref="IWebElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        IWebElement control = await GetTargetControl();
        Actions action = new Actions(ApplicationDetails.WebDriver);
        action.ContextClick(control).Build().Perform();
        logger.Information("Context click performed on control");
    }

}
