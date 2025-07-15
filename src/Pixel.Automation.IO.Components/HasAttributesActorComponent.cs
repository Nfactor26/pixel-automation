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
[ToolBoxItem("Has Attributes", "System.IO", iconSource: null, description: "Get the file attributes of the file on the path.", tags: new string[] { "Has Attributes", "Attributes" })]
public class HasAttributesActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<HasAttributesActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };

    [DataMember]
    [DisplayName("Attribute to Check")]
    [Category("Input")]
    public Argument AttributeToCheck { get; set; } = new InArgument<FileAttributes>() { Mode = ArgumentMode.Default };

    [DataMember]
    [DisplayName("Has Attribute")]
    [Category("Output")]
    public Argument HasAttribute { get; set; } = new OutArgument<bool>() { Mode = ArgumentMode.DataBound };

    public HasAttributesActorComponent() : base("Has Attributes", "HasAttributes")
    {
    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;      
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);      
        var attributeToCheck = await argumentProcessor.GetValueAsync<FileAttributes>(this.AttributeToCheck);
       
        var fileAttributes = File.GetAttributes(path);
        bool hasAttribute = (fileAttributes & attributeToCheck) == attributeToCheck;
        await argumentProcessor.SetValueAsync<bool>(this.HasAttribute, hasAttribute);       
        logger.Information("File {0} has attribute {1}: {2}", path, attributeToCheck, hasAttribute);
    }
}
