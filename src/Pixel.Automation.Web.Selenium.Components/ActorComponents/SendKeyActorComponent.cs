using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// Use <see cref="SendKeyActorComponent"/> to set text on a web control e.g. input
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Send Key", "Selenium", iconSource: null, description: "Send keys to simulate typing on a  WebElement", tags: new string[] { "SendKey", "Type", "Web" })]
public class SendKeyActorComponent : WebElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<SendKeyActorComponent>();

    /// <summary>
    /// Set the text on a web control. To send special keys such as Enter however, use scripted mode and return desired keys. For Example : Keys.Control + \"A\" or Keys.Enter etc.
    /// </summary>
    [DataMember]
    [Display(Name = "Input", GroupName = "Configuration", Order = 20, Description = "Set the text on a web control. To send special keys such as Enter however, " +
        "use scripted mode and return desired keys. For Example : Keys.Control + \"A\" or Keys.Enter etc.")]     
    public Argument Input { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default };

    /// <summary>
    /// Indicates whether to clear the existing value before setting a new value
    /// </summary>
    [DataMember]
    [Display(Name = "Clear", GroupName = "Configuration", Order = 20, Description = "Indicates whether to clear the existing value before setting a new value")]      
    public bool ClearBeforeSendKeys { get; set; } = false;
        
    /// <summary>
    /// Default constructor
    /// </summary>
    public SendKeyActorComponent() : base("Send Key", "SendKey")
    {

    }

    /// <summary>
    /// Set the text on a <see cref="IWebElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        IWebElement control = await GetTargetControl();
        string inputForControl = await ArgumentProcessor.GetValueAsync<string>(this.Input);
        if (this.ClearBeforeSendKeys)
        {
            logger.Information("Value of control was cleared ");
            control.Clear();
        }
        control.SendKeys(inputForControl);
        logger.Information("Send key operation completed on control");

    }
}
