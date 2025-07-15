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
[ToolBoxItem("Read All Text", "System.IO", "File", iconSource: null, description: "Opens a text file, reads all the text in the file into a string, and then closes the file.", tags: new string[] { "Read Text", "Text", "Read", "Read All Text" })]
public class ReadAllTextFileActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<ReadAllTextFileActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The file to open for reading.")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = String.Empty };

    [DataMember]
    [DisplayName("Text")]
    [Description("A string containing all the text in the file.")]
    [Category("Output")]
    public Argument Text { get; set; } = new OutArgument<string>() { Mode = ArgumentMode.DataBound };

    public ReadAllTextFileActorComponent() : base("Read All Text", "ReadAllText")
    {
    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path) ?? string.Empty;
        var text = File.ReadAllText(path);
        await argumentProcessor.SetValueAsync<string>(this.Text, text);
        logger.Information("Read all text from file : {0} ", path);
    }

}
