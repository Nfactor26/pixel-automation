using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Serilog;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="HandlePromptActorComponent"/> to interact with a browser prompt window
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Handle Prompt", "Selenium", "Alerts", iconSource: null, description: "Interact with browser prompot window", tags: new string[] { "prompt", "Web" })]
public class HandlePromptActorComponent : SeleniumActorComponent
{
    private readonly ILogger logger = Log.ForContext<HandlePromptActorComponent>();


    /// <summary>
    /// Indicates whether to accept or dismiss prompt
    /// </summary>
    [DataMember]
    [Display(Name = "Action", GroupName = "Configuration", Order = 10, Description = "Indicates whether to accept or dismiss prompt")]
    public HandleAlertBehavior Action { get; set; } = HandleAlertBehavior.Accept;
  

    /// <summary>
    /// Input value for prompt dialog
    /// </summary>
    [DataMember]
    [Display(Name = "Prompt Value", GroupName = "Configuration", Order = 20, Description = "[Optional] Input for accept prompt action")]     
    [Browsable(false)]
    [RefreshProperties(RefreshProperties.Repaint)]
    public Argument Message { get; set; } = new InArgument<string>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

    /// <summary>
    /// Default constructor
    /// </summary>
    public HandlePromptActorComponent() : base("Handle Prompt", "HandlePrompt")
    {

    }

    /// <summary>
    /// Accept a browser prompt by providing an input value or dismiss the prompt.s
    /// </summary>
    public override async Task ActAsync()
    {
        IAlert alert = ApplicationDetails.WebDriver.SwitchTo().Alert();
        switch (this.Action)
        {
            case HandleAlertBehavior.Accept:
                string input = this.Message.IsConfigured() ? await ArgumentProcessor.GetValueAsync<string>(this.Message) : String.Empty;
                if(this.Message.IsConfigured())
                {
                    alert.SendKeys(input);
                    logger.Information("Prompot value was set.");
                }                
                Thread.Sleep(500);
                alert.Accept();
                logger.Information("Browser prompt was accepted.");
                break;
            case HandleAlertBehavior.Dismiss:
                alert.Dismiss();
                logger.Information("Browser prompt was dismissed.");
                break;
        }
        await Task.CompletedTask;
    }
}
