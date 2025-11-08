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
/// Mapped from <see cref="CF_PIN_STATE"/>"/>
/// </summary>
public enum PinState
{
    /// <summary>The platform can decide freely when the placeholder’s content needs to present or absent locally on the disk.</summary>
    Unspecified,

    /// <summary>
    /// The sync provider will be notified to fetch the placeholder’s content asynchronously after the pin request is received by
    /// the platform. There is no guarantee that the placeholders to be pinned will be fully available locally after a CfSetPinState
    /// call completes successfully. However, the platform will fail any dehydration request on pinned placeholders.
    /// </summary>
    Pinned,

    /// <summary>
    /// The sync provider will be notified to dehydrate/invalidate the placeholder’s content on-disk asynchronously after the unpin
    /// request is received by the platform. There is no guarantee that the placeholders to be unpinned will be fully dehydrated
    /// after the API call completes successfully.
    /// </summary>
    UnPinned,

    /// <summary>
    /// the placeholder will never be synced to the cloud by the sync provider. This state can only be set by the sync provider.
    /// </summary>
    Excluded,

    /// <summary>
    /// The platform treats it as if the caller performs a move operation on the placeholder and hence re-evaluates the
    /// placeholder’s pin state based on its parent’s pin state. See the Remarks section for an inheritance table.
    /// </summary>
    Inherit,
}

/// <summary>
/// Use <see cref="GetPinStatusActorComponent"/> to retrieve the pinned status of a cloud file or directory.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Pin Status", "Cloud File", iconSource: null, description: "Retrieve the pinned status of cloud file or directory", tags: ["Pin", "Status"])]
public class GetPinStatusActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetPinStatusActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The path of the file or directory to check status for")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };

    [DataMember]
    [DisplayName("Status")]
    [Category("Output")]
    [Description("The cloud status of the file or folder")]
    public Argument Status { get; set; } = new OutArgument<PinState>() { Mode = ArgumentMode.Default };

    public GetPinStatusActorComponent() : base("Get Pin Status", "GetPinStatus")
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


        using var hFile = CreateFile(path, 0, FileShare.ReadWrite | FileShare.Delete,
               null, FileMode.Open,
               FileFlagsAndAttributes.FILE_FLAG_OPEN_REPARSE_POINT | FileFlagsAndAttributes.FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
        if (hFile.IsInvalid)
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"CreateFile failed for '{path}'");

        var basicInfo = CfGetPlaceholderInfo<CF_PLACEHOLDER_BASIC_INFO>(hFile);

        await argumentProcessor.SetValueAsync(Status, (PinState)basicInfo.PinState);

        logger.Information("Pin state for path {Path}: {Status}", path, basicInfo.PinState);
    }
}
