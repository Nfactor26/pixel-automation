using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System.ComponentModel;
using System.Runtime.Serialization;
using Windows.Storage;

namespace Pixel.Automations.CloudFile;

/// <summary>
/// Retrieves a custom property from a specified cloud file.
/// Use <see cref="GetCustomPropertyActorComponent"/> to obtain the value of a property by its identifier from a file or directory in the cloud.  
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Properties", "Cloud File", iconSource: null, description: "Retrieve the properties of cloud file", tags: new string[] { "Properties" })]
public class GetCustomPropertyActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetCustomPropertyActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The path of the file or directory")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = String.Empty };

    [DataMember]
    [DisplayName("Property ID")]
    [Category("Input")]
    [Description("The identifier of the property to retrieve")]
    public Argument PropertyId {get; set; } = new InArgument<int>() { Mode = ArgumentMode.Default, DefaultValue = 0 };

    [DataMember]
    [DisplayName("Property Value")]
    [Category("Output")]
    [Description("The value of the property")]
    public Argument PropertyValue { get; set; } = new OutArgument<string>() { Mode = ArgumentMode.Default };

    public GetCustomPropertyActorComponent() : base("Get Custom Property", "GetCustomProperty")
    {
    }

    public async override Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);

        IStorageItem storageItem = null;
        if (File.Exists(path))
        {
            storageItem = await StorageFile.GetFileFromPathAsync(path);
        }
        else if (Directory.Exists(path))
        {
            storageItem = await StorageFolder.GetFolderFromPathAsync(path);
        }
        else
        {
            logger.Error("Path does not exist: {Path}", path);
            throw new FileNotFoundException($"Path does not exist: {path}");
        }
        
        int propertyId = await argumentProcessor.GetValueAsync<int>(this.PropertyId);
        using (var propertyStore = new StorageItemPropertyProvider())
        {
            if (storageItem is IStorageItemProperties storageItemProperties)
            {
                var properties = await propertyStore.GetPropertiesAsync(storageItemProperties);
                if (properties.TryGetValue(propertyId.ToString(), out var property))
                {
                    await argumentProcessor.SetValueAsync(this.PropertyValue, property.Value);
                }
                else
                {
                    logger.Warning("Property ID {PropertyId} not found for item at path {Path}", propertyId, path);
                    await argumentProcessor.SetValueAsync(this.PropertyValue, string.Empty);
                }
            }
        }
    }
}