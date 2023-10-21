using OpenQA.Selenium;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="HandleAlertActorComponent"/> to interact with a browser alert.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Handle Alert", "Selenium", "Alerts", iconSource: null, description: "Accept/Dissmiss alert on web page", tags: new string[] { "Click", "Web" })]
public class HandleAlertActorComponent : SeleniumActorComponent
{
    private readonly ILogger logger = Log.ForContext<HandleAlertActorComponent>();

    /// <summary>
    /// Indicate whether to accept or dismiss the alert
    /// </summary>
    [DataMember]
    [Display(Name = "Action", GroupName = "Configuration", Order = 10, Description = "Indicate whether to accept or dismiss the alert")]       
    public HandleAlertBehavior Action { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public HandleAlertActorComponent() : base("Handle Alert", "HandleAlert")
    {

    }

    /// <summary>
    /// Accept or Dismiss a browser alert
    /// </summary>
    public override async Task ActAsync()
    {
        IAlert alert = ApplicationDetails.WebDriver.SwitchTo().Alert();
        switch(this.Action)
        {
            case HandleAlertBehavior.Accept:
                alert.Accept();
                logger.Information("Alert was accepted");
                break;
            case HandleAlertBehavior.Dismiss:
                alert.Dismiss();
                logger.Information("Alert was dismissed");
                break;
        }
        await Task.CompletedTask;
    }
}    
