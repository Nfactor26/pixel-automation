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
[ToolBoxItem("Hydrate Cloud File", "Cloud File", iconSource: null, description: "Hydrate a cloud file or directory (download contents)", tags: ["Hydrate", "Cloud", "File"])]
public class HydrateActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<HydrateActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The path of the file or directory to hydrate")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };

    public HydrateActorComponent() : base("Hydrate Cloud File", "HydrateCloudFile")
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
        
        var result = CfHydratePlaceholder(hFile, HydrateFlags: CF_HYDRATE_FLAGS.CF_HYDRATE_FLAG_NONE);
        if (result.Failed)
            throw new Win32Exception(result.Code, $"CfHydratePlaceholder failed for '{path}'.");

        logger.Information("Hydrated cloud file or directory at path: {Path}", path);
    }
}
