using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Retrieve a file from the device's file system
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Pull File", "Appium", "Device", iconSource: null, description: "Retrieve a file from the device's file system", tags: new string[] { "pull", "file" })]
public class PullFileActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<PullFileActorComponent>();

    [DataMember]
    [Display(Name = "File To Pull", GroupName = "Input", Order = 10, Description = "File to pull")]
    public Argument FileToPull { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = string.Empty };

    [DataMember]
    [Display(Name = "File Daata", GroupName = "Output", Order = 20, Description = "File data")]
    public Argument FileData { get; set; } = new InArgument<byte[]>() { Mode = ArgumentMode.DataBound, CanChangeType = false, AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted };

    /// <summary>
    /// Default constructor
    /// </summary>
    public PullFileActorComponent() : base("Pull File", "PullFile")
    {

    }

    /// <summary>
    /// Retrieve a file from the device's file system
    /// </summary>
    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var fileToPull = await this.ArgumentProcessor.GetValueAsync<string>(this.FileToPull);
        var fileData = driver.PullFile(fileToPull);
        await this.ArgumentProcessor.SetValueAsync<byte[]>(this.FileData, fileData);
        logger.Information("File : '{0}' was pulled from device", fileToPull);
    }
}
