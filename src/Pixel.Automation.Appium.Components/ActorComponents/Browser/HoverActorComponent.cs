using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="HoverActorComponent"/> to simulate hover on a web control.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Hover", "Appium", "Browser", iconSource: null, description: "Perform a hover action on WebElement", tags: new string[] { "hover" })]

public class HoverActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<HoverActorComponent>();

    /// <summary>
    /// Optional argument which can be used to specify an offset from the of the <see cref="IWebElement"/> where hover action should be performed.
    /// This can be used to lookup an element say div , however, hover on a neighbor element by specifying appropriate offset.
    /// </summary>
    [DataMember]
    [Display(Name = "Offset", GroupName = "Configuration", Order = 10, Description = "[Optional] Specify an offset relative to center of the control for hover position")]      
    public Argument Offset { get; set; } = new InArgument<ScreenCoordinate>() { Mode = ArgumentMode.DataBound, DefaultValue = new ScreenCoordinate() };

    /// <summary>
    /// Default constructor
    /// </summary>
    public HoverActorComponent() : base("Mouse Over", "MouseOver")
    {

    }

    /// <summary>
    /// Simulate a mouse over on a <see cref="IWebElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        IWebElement control = await GetTargetControl();
        Actions action = new Actions(ApplicationDetails.Driver);
        if(this.Offset.IsConfigured())
        {
            var offSet = await this.ArgumentProcessor.GetValueAsync<ScreenCoordinate>(this.Offset);
            action.MoveToElement(control, offSet.XCoordinate, offSet.YCoordinate).Perform();
        }
        else
        {
            action.MoveToElement(control).Perform();
        }
        logger.Information($"Hover on control");
    }

}
