using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Retrieve a folder from the device's file system
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Pull File", "Appium", "Device", iconSource: null, description: "Retrieve a folder from the device's file system", tags: new string[] { "pull", "folder" })]
public class PullFolderActorComponent : AppiumActorComponent
{
    private readonly ILogger logger = Log.ForContext<PullFolderActorComponent>();

    [DataMember]
    [Display(Name = "Folder To Pull", GroupName = "Input", Order = 10, Description = "Folder to pull")]
    public Argument FolrderToPull { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = string.Empty };

    [DataMember]
    [Display(Name = "File Data", GroupName = "Output", Order = 20, Description = "Folder data")]
    public Argument FolrderData { get; set; } = new InArgument<byte[]>() { Mode = ArgumentMode.DataBound, CanChangeType = false, AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted };

    /// <summary>
    /// Default constructor
    /// </summary>
    public PullFolderActorComponent() : base("Pull Folder", "PullFolder")
    {

    }

    /// <summary>
    /// Retrieve a folder from the device's file system
    /// </summary>
    public override async Task ActAsync()
    {
        var driver = this.ApplicationDetails.Driver;
        var folderToPull = await this.ArgumentProcessor.GetValueAsync<string>(this.FolrderToPull);
        var fileData = driver.PullFolder(folderToPull);
        await this.ArgumentProcessor.SetValueAsync<byte[]>(this.FolrderData, fileData);
        logger.Information("Folder : '{0}' was pulled from device", folderToPull);
    }
}
