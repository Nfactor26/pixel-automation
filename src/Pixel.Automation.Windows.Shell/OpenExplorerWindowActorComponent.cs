using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Pixel.Automation.Windows.Shell;

[DataContract]
[Serializable]
[ToolBoxItem("Open Explorer Window", "Windows Shell", iconSource: null, description: "Opens Windows Explorer for a given path", tags: ["Explorer", "Open", "Window"])]
public class OpenExplorerWindowActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<OpenExplorerWindowActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The path to open in Windows Explorer")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };

    public OpenExplorerWindowActorComponent() : base("Open Explorer Window", "OpenExplorerWindow")
    {

    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The specified path is null or empty.");
        }

        // If path is a file, set to parent directory
        if (System.IO.File.Exists(path))
        {
            path = System.IO.Path.GetDirectoryName(path);
        }

        if (!System.IO.Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"The specified path does not exist: {path}");
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
        logger.Information($"Opened Explorer window for path: {path}");
    }
}
