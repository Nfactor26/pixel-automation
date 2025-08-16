using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Use <see cref="HideKeyboardCodeActorComponent"/> to hide soft keyboard
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Hide Keyboard", "Appium", iconSource: null, description: "Hide soft keyboard", tags: new string[] { "hide", "keyboard", "hide keyboard" })]
public class HideKeyboardCodeActorComponent : AppiumElementActorComponent
{
    private readonly ILogger logger = Log.ForContext<HideKeyboardCodeActorComponent>();

    [DataMember]
    [Display(Name = "Key", GroupName = "Input", Order = 10, Description = "[Optional] The button pressed by the mobile driver to attempt hiding the keyboard")]
    public Argument Key { get; set; } = new InArgument<string>() { CanChangeType = false, Mode = ArgumentMode.Default };
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public HideKeyboardCodeActorComponent() : base("Hide Keyboard", "HideKeyboard")
    {

    }

    /// <summary>
    /// Press a particular key on an Android Device
    /// </summary>
    public override async Task ActAsync()
    {
        string key = null;
        if(this.Key.IsConfigured())
        {
            key = await this.ArgumentProcessor.GetValueAsync<string>(this.Key);
        }      
        if(!this.ApplicationDetails.Driver.IsKeyboardShown())
        {
            if(string.IsNullOrEmpty(key))
            {
                this.ApplicationDetails.Driver.HideKeyboard();
            }
            else
            {
                this.ApplicationDetails.Driver.HideKeyboard(key);
            }
            logger.Information("Keyboard was hidden");
        }
    }
}
