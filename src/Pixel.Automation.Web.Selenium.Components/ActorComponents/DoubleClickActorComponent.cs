using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="DoubleClickActorComponent"/> to perform a double click on a web control using selenium webdriver.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Double Click", "Selenium", iconSource: null, description: "Perform a double click action on WebElement", tags: new string[] { "DoubleClick", "Web" })]

public class DoubleClickActorComponent : WebElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<DoubleClickActorComponent>();
 
    /// <summary>
    /// Default constructor
    /// </summary>
    public DoubleClickActorComponent() : base("Double Click", "DoubleClick")
    {

    }

    /// <summary>
    /// Perform a double click on <see cref="IWebElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        Actions action = new Actions(ApplicationDetails.WebDriver);
        action.DoubleClick(control).Perform();
        logger.Information("Double clicked was performed on control : '{0}'", name);
    }

}
