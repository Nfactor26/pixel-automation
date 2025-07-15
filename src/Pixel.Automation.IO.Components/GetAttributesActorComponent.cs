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
[ToolBoxItem("Get Attributes", "System.IO", iconSource: null, description: "Gets the FileAttributes of the file on the path.", tags: new string[] { "Get Attributes", "Attributes" })]
public class GetAttributesActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetAttributesActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = String.Empty };

    [DataMember]
    [DisplayName("Directory Info")]
    [Description("Represents the directory at the specified path.")]
    [Category("Output")]
    public Argument FileAttributes { get; set; } = new OutArgument<FileAttributes>() { Mode = ArgumentMode.DataBound };

    public GetAttributesActorComponent() : base("Get Attributes", "GetAttributes")
    {

    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);

        var fileAttributes = File.GetAttributes(path);      
        await argumentProcessor.SetValueAsync<FileAttributes>(this.FileAttributes, fileAttributes);
        logger.Information("File {0} has attributes {1}", path, fileAttributes.ToString());
    }
}

