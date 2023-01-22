using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="PressKeyCodeActorComponent"/> to press a particular key on an Android Device
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Press Key", "Appium", "Android", iconSource: null, description: "Press a particular key on an Android Device", tags: new string[] { "press", "key", "press key" })]
public class PressKeyCodeActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<PressKeyCodeActorComponent>();
        
    [DataMember]
    [Display(Name = "Keys To Press", GroupName = "Input", Order = 10, Description = "Keys to press")]
    public Argument KeysToPress { get; set; } = new InArgument<IEnumerable<KeyEvent>>() { CanChangeType = false, AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Indicates whether to clear the existing value before setting a new value
    /// </summary>
    [DataMember]
    [Display(Name = "Long Press", GroupName = "Input", Order = 20, Description = "Indicates whether to long press")]
    public bool LongPress { get; set; } = false;

    /// <summary>
    /// Default constructor
    /// </summary>
    public PressKeyCodeActorComponent() : base("Press Key", "PressKeyCode")
    {

    }

    /// <summary>
    /// Press a particular key on an Android Device
    /// </summary>
    public override async Task ActAsync()
    {       
        IEnumerable<KeyEvent> keysToPress = await ArgumentProcessor.GetValueAsync<IEnumerable<KeyEvent>>(this.KeysToPress);
        if(this.ApplicationDetails.Driver is AndroidDriver androidDriver)
        {
            foreach (var key in keysToPress)
            {
                if (this.LongPress)
                {
                    androidDriver.LongPressKeyCode(key);                   
                }
                else
                {
                    androidDriver.PressKeyCode(key);
                }
                logger.Information("Key was pressed");
            }
        }
        throw new NotSupportedException($"{nameof(PressKeyCodeActorComponent)} is only supported on android device");
    }
}
