using OpenQA.Selenium.Appium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="SendKeyActorComponent"/> to set text on a web control e.g. input
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Send Key", "Appium", iconSource: null, description: "Send keys to simulate typing on a AppiumElement", tags: new string[] { "send key", "key" })]
public class SendKeyActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<SendKeyActorComponent>();

    /// <summary>
    /// Set the text on a control.
    /// </summary>
    [DataMember]
    [Display(Name = "Input", GroupName = "Input", Order = 10, Description = "Set the text on a control")]     
    public Argument Input { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default };

    /// <summary>
    /// Indicates whether to clear the existing value before setting a new value
    /// </summary>
    [DataMember]
    [Display(Name = "Clear", GroupName = "Input", Order = 20, Description = "Indicates whether to clear the existing value before setting a new value")]      
    public bool ClearBeforeSendKeys { get; set; } = false;
        
    /// <summary>
    /// Default constructor
    /// </summary>
    public SendKeyActorComponent() : base("Send Key", "SendKey")
    {

    }

    /// <summary>
    /// Set the text on a <see cref="AppiumElement"/>
    /// </summary>
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        string inputForControl = await ArgumentProcessor.GetValueAsync<string>(this.Input);
        if (this.ClearBeforeSendKeys)
        {
            logger.Information("Value of control : '{0}' was cleared", name);
            control.Clear();
        }
        control.SendKeys(inputForControl);
        logger.Information("Send key operation completed on control : '{0}'", name);

    }
}
