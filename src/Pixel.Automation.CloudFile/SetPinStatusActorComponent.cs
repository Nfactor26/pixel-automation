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

[DataContract]
[Serializable]
[ToolBoxItem("Set Pin Status", "Cloud File", iconSource: null, description: "Set Pin Status on a cloud file e.g. Pinned or UnPinned", tags: ["Pin", "Cloud", "File"])]
public class SetPinStatusActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<SetPinStatusActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The path of the file or directory on which pin state needs to be changed")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };

    [DataMember]
    [DisplayName("Pin Status")]
    [Category("Input")]
    [Description("Pin Status to set. Set Pinned to 'Always Keep On Device' or UnPinned to 'Free Up Space'.")]
    public Argument PinStatus { get; set; } = new InArgument<PinState>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound, CanChangeType = false };

    public SetPinStatusActorComponent() : base("Set Pin Status", "SetPinStatus")
    {
    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);
        PinState pinState = await argumentProcessor.GetValueAsync<PinState>(this.PinStatus);

        using var hFind = FindFirstFile(path, out WIN32_FIND_DATA findData);
        if (hFind.IsInvalid)
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"FindFirstFile failed for '{path}'.");

        using var hFile = CreateFile(path, 0, FileShare.ReadWrite | FileShare.Delete,
           null, FileMode.Open,
           FileFlagsAndAttributes.FILE_FLAG_OPEN_REPARSE_POINT | FileFlagsAndAttributes.FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
        if (hFile.IsInvalid)
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"CreateFile failed for '{path}'");

        if(pinState == PinState.Excluded)
            throw new InvalidOperationException("PinState.Excluded can only be set by the sync provider.");

        var result = CfSetPinState(hFile, (CF_PIN_STATE)pinState, CF_SET_PIN_FLAGS.CF_SET_PIN_FLAG_NONE, IntPtr.Zero);
        if (result.Failed)
            throw new Win32Exception(result.Code, $"CfSetPinState failed for '{path}'.");

        logger.Information("Marked cloud file or directory as always available on device at path: {Path}", path);
    }
}
