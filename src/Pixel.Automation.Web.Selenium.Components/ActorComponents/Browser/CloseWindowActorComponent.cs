using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="CloseWindowActorComponent"/> to close browser window or tab.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Close Window", "Selenium", "Browser", iconSource: null, description: "Close a tab or window", tags: new string[] { "Close", "Tab" , "Window", "Web" })]
public class CloseWindowActorComponent : SeleniumActorComponent
{
    private readonly ILogger logger = Log.ForContext<CloseWindowActorComponent>();

    /// <summary>
    /// Specify the index of the window or tab to be closed. Default tab/window has index of 1 and can't be closed.
    /// </summary>
    [DataMember]
    [Display(Name = "Window Number", GroupName = "Configuration", Order = 10, Description = "Index (1 based) of the tab/window to be closed. " +
        "Default tab/window can't be closed")]     
    public Argument WindowNumber { get; set; } = new InArgument<int>() { DefaultValue = 2 };

    /// <summary>
    /// Default constructor
    /// </summary>
    public CloseWindowActorComponent() : base("Close Window", "CloseWindow")
    {           

    }

    /// <summary>
    /// Close the window or tab specified using WindowNumber for a given browser opened using selenium webdriver.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">Throws index out of range exception if the window or tab to be closed doesn't exist</exception>
    public override async Task ActAsync()
    {
        IWebDriver webDriver = ApplicationDetails.WebDriver;
        int windowNumber = await ArgumentProcessor.GetValueAsync<int>(this.WindowNumber) - 1;
        if (windowNumber > 0 && webDriver.WindowHandles.Count() > windowNumber)
        {               
            webDriver.SwitchTo().Window(webDriver.WindowHandles[windowNumber]);
            webDriver.Close();
            logger.Information("Window / Tab at index {windowNumber + 1} was closed.");
            await Task.CompletedTask;
            return;
        }

        throw new IndexOutOfRangeException($"Only {webDriver.WindowHandles.Count} windows / tabs are open. Can't close configured window  at index : {this.WindowNumber}");
    }     

}
