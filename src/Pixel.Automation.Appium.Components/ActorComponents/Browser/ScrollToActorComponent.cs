﻿using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="ScrollToActorComponent"/> to scroll to a target web control
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Scroll To Element", "Appium", "Browser", iconSource: null, description: "Scroll to web control in browser window", tags: new string[] { "scroll" })]

public class ScrollToActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<ScrollToActorComponent>();

    /// <summary>
    /// Amount of vertial offset from element's actual y coordinate to apply while scrolling
    /// </summary>
    [DataMember]      
    [Display(Name = "Offset", GroupName = "Configuration", Order = 10, Description = "Amount of vertial offset from element's actual y coordinate to apply while scrolling")]       
    public Argument OffSet { get; set; } = new InArgument<int>() { DefaultValue = 0 };

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
        int offsetAmount = await ArgumentProcessor.GetValueAsync<int>(this.OffSet);
        IWebElement control = await GetTargetControl();
        int elemPos = control.Location.Y+ offsetAmount;
        ((IJavaScriptExecutor)ApplicationDetails.Driver).ExecuteScript($"window.scroll(0, {elemPos});");

        logger.Information($"Browser was scrolled to (0, {elemPos})");
    }
}