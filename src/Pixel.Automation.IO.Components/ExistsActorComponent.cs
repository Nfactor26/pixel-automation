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
[ToolBoxItem("Exists", "System.IO", iconSource: null, description: "Checks if a file or directory exists.", tags: new string[] { "Exists", "File", "Directory" })]
public class ExistsActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<ExistsActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };

    [DataMember]
    [DisplayName("Exists")]
    [Category("Output")]
    public Argument Exists { get; set; } = new OutArgument<bool>() { Mode = ArgumentMode.DataBound };

    public ExistsActorComponent() : base("Exists", "Exists")
    {

    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);
      
        bool exists = System.IO.Path.Exists(path);     
        await argumentProcessor.SetValueAsync<bool>(this.Exists, exists);
        logger.Information("Path '{0}' exists: {1}", path, exists);
    }
}