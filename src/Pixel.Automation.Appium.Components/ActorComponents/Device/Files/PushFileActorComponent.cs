using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Place a file onto the device in a particular place
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Push File", "Appium", "Device", iconSource: null, description: "Place a file onto the device in a particular place", tags: new string[] { "push", "file" })]
public class PushFileActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<PushFileActorComponent>();

    [DataMember]
    [Display(Name = "File", GroupName = "Input", Order = 10, Description = "File to push")]
    public Argument FileToPush { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = string.Empty };

    [DataMember]
    [Display(Name = "Path On Device", GroupName = "Input", Order = 20, Description = "Path on Device")]
    public Argument TargetDirectory { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = string.Empty };

    /// <summary>
    /// Default constructor
    /// </summary>
    public PushFileActorComponent() : base("Push File", "PushFile")
    {

    }

    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var fileToPush = await this.ArgumentProcessor.GetValueAsync<string>(this.FileToPush);
        if(!File.Exists(fileToPush))
        {
            throw new FileNotFoundException($"File : '{fileToPush}' doesn't exist");
        }
        var pathOnDevice = await this.ArgumentProcessor.GetValueAsync<string>(this.TargetDirectory);
        driver.PushFile(pathOnDevice, fileToPush);
        logger.Information("File : '{0}' was pushed on device at path : '{1}'", fileToPush, pathOnDevice);
    }
}
