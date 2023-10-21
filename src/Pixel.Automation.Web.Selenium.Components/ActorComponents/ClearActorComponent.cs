using OpenQA.Selenium;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="ClickActorComponent"/> to clear the value of a control.  
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Clear", "Selenium", iconSource: null, description: "Clear the value of WebElement", tags: new string[] { "Clear", "Web" })]
public class ClearActorComponent : WebElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<ClearActorComponent>();         
  

    public ClearActorComponent() : base("Clear", "Clear")
    {

    }

    /// <summary>
    /// Clear the value of <see cref="IWebElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        control.Clear();
        logger.Information("Value of control : '{0}' was cleared", name);
    }
}
