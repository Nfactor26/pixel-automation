using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Vanara.PInvoke;
using static Vanara.PInvoke.CldApi;
using static Vanara.PInvoke.Kernel32;

namespace Pixel.Automation.CloudFile;


/// <summary>
/// Mapped from <see cref="CF_PLACEHOLDER_STATE"/>"/>
/// </summary>
[Flags]
public enum PlaceHolderState : uint
{
    /// <summary>
    ///  When returned, the file or directory whose FileAttributes and ReparseTag examined
    ///  by the API is not a placeholder.
    /// </summary>
    NoStates = 0u,

    /// <summary>
    /// The file or directory whose FileAttributes and ReparseTag examined by the API
    /// is a placeholder.
    /// </summary>
    Placeholder = 1u,

    /// <summary>
    /// The directory is both a placeholder directory as well as the sync root.
    /// </summary>
    SyncRoot = 2u,

    /// <summary>
    /// The file or directory must be a placeholder and there exists an essential property
    /// in the property store of the file or directory.
    /// </summary>
    EssentialPropPresent = 4u,

    /// <summary>
    /// The file or directory must be a placeholder and its content in sync with the
    /// cloud.
    /// </summary>
    InSync = 8u,

    /// <summary>
    /// The file or directory must be a placeholder and its content is not ready to be
    /// consumed by the user application, though it may or may not be fully present locally.
    /// An example is a placeholder file whose content has been fully downloaded to the
    /// local disk, but is yet to be validated by a sync provider that has registered
    /// the sync root with the hydration modifier VERIFICATION_REQUIRED.
    /// </summary>
    Partial = 0x10u,

    /// <summary>
    /// The file or directory must be a placeholder and its content is not fully present
    /// locally. When this is set, CF_PLACEHOLDER_STATE_PARTIAL must also be set.
    /// </summary>
    PartiallyOnDisk = 0x20u,

    /// <summary>
    ///  This is an invalid state when the API fails to parse the information of the file
    ///  or directory.
    /// </summary>
    Invalid = uint.MaxValue
}

/// <summary>
/// Use <see cref="GetPlaceholderStateActorComponent"/> to retrieve the status of a cloud file or directory.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Status", "Cloud File", iconSource: null, description: "Retrieve the status of cloud file or directory", tags: ["Status"])]
public class GetPlaceholderStateActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetPlaceholderStateActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The path of the file or directory to check status for")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };

    [DataMember]
    [DisplayName("Status")]
    [Category("Output")]
    [Description("The cloud status of the file or folder")]
    public Argument Status { get; set; } = new OutArgument<PlaceHolderState>() { Mode = ArgumentMode.Default };

    public GetPlaceholderStateActorComponent() : base("Get Status", "GetStatus")
    {
    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);

        // Using FindFirstFile to obtain WIN32_FIND_DATA without opening file. It doesn’t hydrate or modify the file—it just queries its metadata
        using var hFind = FindFirstFile(path, out WIN32_FIND_DATA findData);
        if (hFind.IsInvalid)
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"FindFirstFile failed for '{path}'.");            
        var status =  CfGetPlaceholderStateFromFindData(in findData);   
       
        await argumentProcessor.SetValueAsync(Status, (PlaceHolderState)status);
        
        logger.Information("Status for path {Path}: {Status}", path, status);
    }
}
