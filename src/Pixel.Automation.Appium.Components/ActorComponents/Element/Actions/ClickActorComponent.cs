using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="ClickActorComponent"/> to perform a click operation on control.  
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Click", "Appium", iconSource: null, description: "Perform a click action on AppiumElement", tags: new string[] { "click" })]
public class ClickActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<ClickActorComponent>();         
   
    public ClickActorComponent() : base("Click", "Click")
    {

    }

    /// <summary>
    /// Perform a click on <see cref="AppiumElement"/> using Click() method.  
    /// </summary>
    public override async Task ActAsync()
    {
        IWebElement control = await GetTargetControl();
        control.Click();
        logger.Information("control was clicked.");
    }
}
