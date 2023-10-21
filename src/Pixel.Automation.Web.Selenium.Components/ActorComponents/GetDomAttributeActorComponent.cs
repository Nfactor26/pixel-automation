using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="GetDomAttributeActorComponent"/> to retrieve the Dom attribute from a web control.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Dom Attribute", "Selenium", iconSource: null, description: "Get the value of the dom attribute of a WebElement", tags: new string[] { "Dom", "Dom Attribute", "Get", "Web" })]
public class GetDomAttributeActorComponent : WebElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetDomAttributeActorComponent>();

    /// <summary>
    /// Name of the attribute whose value needs to be retrieved
    /// </summary>
    [DataMember(IsRequired = true)]
    [Display(Name = "Dom Attribute Name", GroupName = "Configuration", Order = 20, Description = "Name of the Dom attribute whoe value needs to be retrieved")]
    public Argument AttributeName { get; set; } = new InArgument<string>() { DefaultValue = "checked", Mode = ArgumentMode.Default };

    /// <summary>
    /// Argument where the value of the attribute will be stored
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Store the result in specified Argument")]       
    public Argument Result { get; set; } = new OutArgument<string>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetDomAttributeActorComponent() : base("Get Dom Attribute", "GetDomAttribute")
    {

    }

    /// <summary>
    /// Retrieve the value of configured attribute from <see cref="IWebElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        var attributeName = await this.ArgumentProcessor.GetValueAsync<string>(this.AttributeName);
        string extractedValue = control.GetDomAttribute(attributeName);
        await ArgumentProcessor.SetValueAsync<string>(Result, extractedValue);
        logger.Information("Retrived Dom attribue : '{0}' from control : '{1}'", attributeName, name);
    }

}
