using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="GetAttributeActorComponent"/> to get the value of an element's attribute
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Attribute", "Appium", iconSource: null, description: "Get the value of an element's attribute", tags: new string[] { "get", "attribute" })]
public class GetAttributeActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetAttributeActorComponent>();

    /// <summary>
    /// Name of the attribute whose value needs to be retrieved
    /// </summary>
    [DataMember(IsRequired = true)]
    [Display(Name = "Attribute Name", GroupName = "Input", Order = 10, Description = "Name of the attribute whose value needs to be retrieved")]
    public Argument AttributeName { get; set; } = new InArgument<string>() { DefaultValue = "value", Mode = ArgumentMode.Default };

    /// <summary>
    /// Argument where the value of the attribute will be stored
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Store the result in specified Argument")]       
    public Argument Result { get; set; } = new OutArgument<string>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetAttributeActorComponent() : base("Get Attribute", "GetAttribute")
    {

    }

    /// <summary>
    /// Retrieve the value of configured attribute from <see cref="IWebElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        var attributeName = await this.ArgumentProcessor.GetValueAsync<string>(this.AttributeName);
        string extractedValue = control.GetAttribute(attributeName);
        await ArgumentProcessor.SetValueAsync<string>(Result, extractedValue);
        logger.Information("Retrived  attribue : '{0}' having value : '{1}' from control : '{2}'", attributeName, extractedValue, name);
    }

}
