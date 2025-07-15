using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.IO.Components;

[DataContract]
[Serializable]
[ToolBoxItem("Delete", "System.IO", iconSource: null, description: "Deletes a file or specified directory, and optionally any subdirectories", tags: new string[] { "Delete", "File", "Directory" })]
public class DeleteActorComponent : IOActorComponent
{
    private readonly ILogger logger = Log.ForContext<CreateDirectoryActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The path of the file or directory to delete")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = String.Empty };

    [DataMember]
    [DisplayName("Recursive")]
    [Category("Input")]
    [Description("True to remove directories, subdirectories, and files in path; otherwise, false")]
    public Argument Recursive { get; set; } = new InArgument<bool>() { Mode = ArgumentMode.Default, DefaultValue = true };

    public DeleteActorComponent() : base("Delete", "Delete")
    {
         
    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);
        bool recursive = await argumentProcessor.GetValueAsync<bool>(this.Recursive);

        ThrowIfPathNotExists(path);

        if (File.Exists(path))
        {
            File.Delete(path);
            logger.Information("File {0} was deleted", path);       
        }
        else
        {
            Directory.Delete(path, recursive);
            logger.Information("Directory {0} was deleted", path);
        }       
    }
}
