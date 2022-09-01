using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Web.Selenium.Components.Enums;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="NewWindowActorComponent"/> to open a new window or tab.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("New Window", "Selenium", "Browser", iconSource: null, description: "Open a new tab or a window", tags: new string[] { "New", "Tab", "Window", "Web" })]
public class NewWindowActorComponent : SeleniumActorComponent
{
    private readonly ILogger logger = Log.ForContext<NewWindowActorComponent>();

    [DataMember(IsRequired = true)]
    [Display(Name = "Window Type", GroupName = "Configuration", Order = 10, Description = "Indicates whether to open a new tab or a window")]        
    public BrowserWindowType WindowType { get; set; } = BrowserWindowType.Tab;

    [DataMember]
    [Display(Name = "Navigate To", GroupName = "Configuration", Order = 20, Description = "[Optional] Url to navigate to after opening new window/tab")]    
    public Argument TargetUrl { get; set; } = new InArgument<Uri>() { DefaultValue = new Uri("https://www.bing.com") };

    /// <summary>
    /// Default constructor
    /// </summary>
    public NewWindowActorComponent() : base("New Window", "NewWindow")
    {

    }

    /// <summary>
    /// Open a new window or tab for the browser and navigate new window or tab to configured url.
    /// </summary>
    public override async Task ActAsync()
    {
        IWebDriver webDriver = ApplicationDetails.WebDriver;
        switch(this.WindowType)
        {
            case BrowserWindowType.Tab:
                webDriver.SwitchTo().NewWindow(OpenQA.Selenium.WindowType.Tab);
                break;
            case BrowserWindowType.Window:
                webDriver.SwitchTo().NewWindow(OpenQA.Selenium.WindowType.Window);
                break;
        }
        logger.Information($"A new {this.WindowType} was opened.");
        if(this.TargetUrl.IsConfigured())
        {
            Uri targetUrl = await ArgumentProcessor.GetValueAsync<Uri>(this.TargetUrl);
            webDriver.Navigate().GoToUrl(targetUrl);
            logger.Information($"Navigated to Uri : {targetUrl}");
        }
        await Task.CompletedTask;
    }

}
