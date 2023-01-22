using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="DoubleClickActorComponent"/> to perform a double click on a web control using selenium webdriver.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Double Click", "Appium", "Browser", iconSource: null, description: "Perform a double click action on WebElement", tags: new string[] { "double", "click" })]

public class DoubleClickActorComponent : AppiumElementActorComponent
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
        IWebElement control = await GetTargetControl();
        Actions action = new Actions(ApplicationDetails.Driver);
        action.DoubleClick(control).Perform();
        logger.Information("control was double clicked.");
    }

}
