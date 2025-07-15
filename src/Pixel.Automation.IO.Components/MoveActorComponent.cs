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
[ToolBoxItem("Move", "System.IO", iconSource: null, description: "Moves a file or a directory and its contents to a new location", tags: new string[] { "Move Directory", "Move File" })]
public class MoveActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<MoveActorComponent>();

    [DataMember]
    [DisplayName("Source Path")]
    [Category("Input")]
    [Description("The path of the file or directory to move")]
    public Argument SourcePath { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = String.Empty };

    [DataMember]
    [DisplayName("Destination Path")]
    [Category("Input")]
    [Description("The path to the new location for source path or its contents. If source path is a file, then destination path must also be a file name")]
    public Argument DestPath { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = String.Empty };

    public MoveActorComponent() : base("Move", "Move")
    {

    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string sourceDirName = await argumentProcessor.GetValueAsync<string>(this.SourcePath);
        string destDirName = await argumentProcessor.GetValueAsync<string>(this.DestPath);        
        Directory.Move(sourceDirName, destDirName);
        logger.Information("Directory/File {0} was moved to location {1}", sourceDirName, destDirName);

    }
}
