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

public enum SyncState
{
    /// <summary>
    /// The platform clears the placeholder’s in-sync state upon a successful return from the CfSetInSyncState call.
    /// </summary>
    NotInSync,

    /// <summary>
    /// The platform sets the placeholder’s in-sync state upon a successful return from the CfSetInSyncState call.
    /// </summary>
    InSync,
}

/// <summary>
/// Use <see cref="GetSyncStatusActorComponent"/> to retrieve the pinned status of a cloud file or directory.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Sync Status", "Cloud File", iconSource: null, description: "Retrieve the sync status of cloud file or directory", tags: ["Sync", "Status"])]
public class GetSyncStatusActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetSyncStatusActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The path of the file or directory to check status for")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };

    [DataMember]
    [DisplayName("Status")]
    [Category("Output")]
    [Description("The cloud status of the file or folder")]
    public Argument Status { get; set; } = new OutArgument<SyncState>() { Mode = ArgumentMode.Default };

    public GetSyncStatusActorComponent() : base("Get Sync Status", "GetSyncStatus")
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

        await argumentProcessor.SetValueAsync(Status, (SyncState)basicInfo.InSyncState);

        logger.Information("Sync status for path {Path}: {Status}", path, basicInfo.InSyncState);
    }
}
