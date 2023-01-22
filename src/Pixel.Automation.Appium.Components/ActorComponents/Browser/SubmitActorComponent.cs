using OpenQA.Selenium;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="SubmitActorComponent"/> to perform submit on a <see cref="IWebElement"/>
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Submit", "Appium", "Browser", iconSource: null, description: "Perform submit on a form", tags: new string[] { "submit" })]
public class SubmitActorComponent : AppiumElementActorComponent
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
        IWebElement control = await GetTargetControl();
        control.Submit();
        logger.Information("Submit executed on control");
    }
}
