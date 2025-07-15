using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.IO.Components;

[DataContract]
[Serializable]
[ToolBoxItem("Create Directory", "System.IO", "Directory", iconSource: null, description: "Creates all directories and subdirectories in the specified path unless they already exist", tags: new string[] { "Create Directory" })]
public class CreateDirectoryActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<CreateDirectoryActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = String.Empty };

    [DataMember]
    [DisplayName("Directory Info")]
    [Description("Represents the directory at the specified path.")]
    [Category("Output")]
    public Argument DirectoryInfo { get; set; } = new OutArgument<DirectoryInfo>() { Mode = ArgumentMode.DataBound };

    public CreateDirectoryActorComponent() : base("Create Directory", "CreateDirectory")
    {
         
    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path) ?? string.Empty;
        var directoryInfo = Directory.CreateDirectory(path);
        logger.Information("Directory {0} created", path);
        if(this.DirectoryInfo.IsConfigured())
        {
            await argumentProcessor.SetValueAsync<DirectoryInfo>(this.DirectoryInfo, directoryInfo);
        }
    }
}
