using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

[DataContract]
[Serializable]
[ToolBoxItem("Scroll Window", "Selenium", iconSource: null, description: "Scroll the window to a particular place in the document", tags: new string[] { "Click", "Web" })]

public class ScrollWindowActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<ScrollWindowActorComponent>();

    /// <summary>
    /// HorizontalScroll is the pixel along the horizontal axis of the document that you want displayed in the upper left.
    /// </summary>
    [DataMember]
    [Display(Name = "X-coord", GroupName = "Configuration", Order = 30, Description = "HorizontalScroll is the pixel along the horizontal" +
        " axis of the document that you want displayed in the upper left.")]
    public Argument XCoord { get; set; } = new InArgument<int>() { Mode = ArgumentMode.Default, DefaultValue = 0 };

    /// <summary>
    /// VerticalScroll is the pixel along the vertical axis of the document that you want displayed in the upper left.
    /// </summary>
    [DataMember]
    [Display(Name = "Y-coord", GroupName = "Configuration", Order = 40, Description = "VerticalScroll is the pixel along the vertical " +
        "axis of the document that you want displayed in the upper left.")]
    public Argument YCoord { get; set; } = new InArgument<int>() { Mode = ArgumentMode.Default, DefaultValue = 100 };

    /// <summary>
    /// Default constructor
    /// </summary>
    public ScrollWindowActorComponent() : base("Scroll Window", "ScrollWindow")
    {

    }

    /// <summary>
    /// Scroll the browser horizontally or vertically or both.
    /// </summary>
    public override async Task ActAsync()
    {
        int yCoord = await this.ArgumentProcessor.GetValueAsync<int>(this.YCoord);
        int xCoord = await this.ArgumentProcessor.GetValueAsync<int>(this.XCoord);
        _ = ((IJavaScriptExecutor)ApplicationDetails.Driver).ExecuteScript($"window.scroll({xCoord}, {yCoord});");
        logger.Information("Window was scrolled to '({0},{1})'", xCoord, yCoord);
        await Task.CompletedTask;
    }

}
