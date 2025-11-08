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
[ToolBoxItem("Dehydrate Cloud File", "Cloud File", iconSource: null, description: "Dehydrate a cloud file or directory (remove local content)", tags: ["Dehydrate", "Cloud", "File"])]
public class DehydrateActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<DehydrateActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The path of the file or directory to dehydrate")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };

    public DehydrateActorComponent() : base("Dehydrate Cloud File", "DehydrateCloudFile")
    {
    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);

        using var hFind = FindFirstFile(path, out WIN32_FIND_DATA findData);
        if (hFind.IsInvalid)
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"FindFirstFile failed for '{path}'.");

        using var hFile = CreateFile(path, 0, FileShare.ReadWrite | FileShare.Delete,
           null, FileMode.Open,
           FileFlagsAndAttributes.FILE_FLAG_OPEN_REPARSE_POINT | FileFlagsAndAttributes.FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
        if (hFile.IsInvalid)
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"CreateFile failed for '{path}'");

        var result = CfDehydratePlaceholder(hFile, 0, -1, DehydrateFlags : CF_DEHYDRATE_FLAGS.CF_DEHYDRATE_FLAG_NONE);
        if (result.Failed)
            throw new Win32Exception(result.Code, $"CfDehydratePlaceholder failed for '{path}'.");

        logger.Information("Dehydrated cloud file or directory at path: {Path}", path);
    }
}
