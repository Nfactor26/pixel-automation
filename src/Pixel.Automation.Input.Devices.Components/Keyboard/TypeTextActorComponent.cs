using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices.Components;

/// <summary>
/// Use <see cref="TypeTextActorComponent"/> to simulate typing some text.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Type text", "Input Device", "Keyboard", iconSource: null, description: "Type text using keyboard", tags: new string[] { "Type" })]
public class TypeTextActorComponent : InputDeviceActor
{
    private readonly ILogger logger = Log.ForContext<TypeTextActorComponent>();

    [DataMember]
    [Display(Name = "Text To Type", GroupName = "Configuration", Order = 10, Description = "Text to be typed")]
    public Argument Input { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = "", CanChangeType = false };

    /// <summary>
    /// Default constructor
    /// </summary>
    public TypeTextActorComponent() : base("Type text", "TypeText")
    {

    }

    /// <summary>
    /// Type configured text by simulating keyboard
    /// </summary>
    public override async Task ActAsync()
    {           
        string textToType = await this.ArgumentProcessor.GetValueAsync<string>(this.Input);

        if (!string.IsNullOrEmpty(textToType))
        {
            var keyboard = GetKeyboard();
            keyboard.TypeText(textToType);
        }
        logger.Information("Text : '{0}' was typed", textToType);
        await Task.CompletedTask;
    }

}
