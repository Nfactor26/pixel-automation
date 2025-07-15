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
[ToolBoxItem("Set Attributes", "System.IO", iconSource: null, description: "Sets the specified FileAttributes of the file on the specified path.", tags: new string[] { "Set Attributes", "Attributes" })]
public class SetAttributesActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<SetAttributesActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = String.Empty };

    [DataMember]
    [DisplayName("Attributes")]
    [Description("A bitwise combination of the enumeration values")]
    [Category("Input")]
    public Argument FileAttributes { get; set; } = new OutArgument<FileAttributes>() { Mode = ArgumentMode.DataBound };

    public SetAttributesActorComponent() : base("Set Attributes", "SetAttributes")
    {
    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);
        var attributes = await argumentProcessor.GetValueAsync<FileAttributes>(this.FileAttributes);
        File.SetAttributes(path, attributes);       
        logger.Information("File {0} has attributes {1} after applying new attributes", path, File.GetAttributes(path).ToString());
    }
}
