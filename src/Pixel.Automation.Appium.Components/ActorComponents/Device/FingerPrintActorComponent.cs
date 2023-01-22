using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components.ActorComponents;

/// <summary>
/// Authenticate users by using their finger print scans on supported emulators
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Finger Print", "Appium", "Device", iconSource: null, description: "Authenticate users by using their finger print scans on supported emulators", tags: new string[] { "finger", "print" })]
public class FingerPrintActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<FingerPrintActorComponent>();

    [DataMember]
    [Display(Name = "Finger Print ID", GroupName = "Input", Order = 10, Description = "Finger Print ID to use")]
    public Argument FingerPrintID { get; set; } = new InArgument<int>() { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = 1 };

    /// <summary>
    /// Default constructor
    /// </summary>
    public FingerPrintActorComponent() : base("Finger Print", "FingerPrint")
    {

    }

    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var fingerPrintID = await this.ArgumentProcessor.GetValueAsync<int>(this.FingerPrintID);
        driver.FingerPrint(fingerPrintID);
        logger.Information("User was authenticated using finger print ID : {0}", fingerPrintID);
    }
}
