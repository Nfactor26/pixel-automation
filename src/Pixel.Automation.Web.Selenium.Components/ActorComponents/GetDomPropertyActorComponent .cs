using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="GetDomPropertyActorComponent"/> to retrieve the value of any Dom property from a web control.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Dom Property", "Selenium", iconSource: null, description: "Get the value of given Dom property of a WebElement", tags: new string[] { "Attribute", "Attribute value", "Value", "Get", "Web" })]
public class GetDomPropertyActorComponent : WebElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetDomPropertyActorComponent>();

    /// <summary>
    /// Name of the attribute whose value needs to be retrieved
    /// </summary>
    [DataMember(IsRequired = true)]
    [Display(Name = "Dom Property", GroupName = "Configuration", Order = 20, Description = "Name of the Dom Property")]
    public Argument PropertyName { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default };

    /// <summary>
    /// Argument where the value of the attribute will be stored
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Store the result in specified Argument")]       
    public Argument Result { get; set; } = new OutArgument<string>();

    /// <summary>
    /// Default constructor
    /// </summary>
    public GetDomPropertyActorComponent() : base("Get Dom Property", "GetDomProperty")
    {

    }

    /// <summary>
    /// Retrieve the value of configured attribute from <see cref="IWebElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        var propertyName = await this.ArgumentProcessor.GetValueAsync<string>(this.PropertyName);
        string extractedValue = control.GetDomProperty(propertyName);
        await ArgumentProcessor.SetValueAsync<string>(Result, extractedValue);
        logger.Information("Retrived Dom property : '{0}' from control '{1}'", propertyName, name);
    }

}
