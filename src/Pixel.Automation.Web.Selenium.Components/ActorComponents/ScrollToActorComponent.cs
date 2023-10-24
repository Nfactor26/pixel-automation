using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="ScrollToActorComponent"/> to scroll to a target web control
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Scroll To Element", "Selenium", iconSource: null, description: "Scroll to web control in browser window", tags: new string[] { "Scroll", "Web" })]

public class ScrollToActorComponent : WebElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<ScrollToActorComponent>();

    /// <summary>
    /// Amount of vertial offset from element's actual y coordinate to apply while scrolling
    /// </summary>
    [DataMember]      
    [Display(Name = "X-Offset", GroupName = "Configuration", Order = 20, Description = "Amount of horizontal offset from element's actual x coordinate to apply while scrolling")]       
    public Argument XOffSet { get; set; } = new InArgument<int>() { DefaultValue = 0 };

    [DataMember]
    [Display(Name = "Y-Offset", GroupName = "Configuration", Order = 20, Description = "Amount of vertial offset from element's actual y coordinate to apply while scrolling")]
    public Argument YOffSet { get; set; } = new InArgument<int>() { DefaultValue = 0 };

    /// <summary>
    /// Default constructor
    /// </summary>
    public ScrollToActorComponent() : base("Scroll To Element", "ScrollToElement")
    {

    }

    /// <summary>
    /// Scroll the browser to a target <see cref="IWebElement"/>.
    /// A vertical offset can be optionally specified.
    /// </summary>
    public override async Task ActAsync()
    {           
        int xOffsetAmount = await ArgumentProcessor.GetValueAsync<int>(this.XOffSet);
        int yOffsetAmount = await ArgumentProcessor.GetValueAsync<int>(this.YOffSet);
        var (name, control) = await GetTargetControl();      
        ((IJavaScriptExecutor)ApplicationDetails.WebDriver).ExecuteScript($"window.scroll({control.Location.X + xOffsetAmount}, {control.Location.Y + yOffsetAmount});");
        logger.Information("Browser was scrolled to control : '{0}' having location '({1}, {2})'. Offset used was '({3}, {4})'", name, control.Location.X, xOffsetAmount, control.Location.Y, yOffsetAmount);
    }
}
