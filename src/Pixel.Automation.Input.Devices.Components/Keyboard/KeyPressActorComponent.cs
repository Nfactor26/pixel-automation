using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices.Components
{
    /// <summary>
    /// Use <see cref="KeyPressActorComponent"/> to simulate pressing a key e.g. A , F5, etc.
    /// Warning : A key down must be followed by a Key up for same set of keys. If this is not done, It will leave keyboard state in a corrupted state.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Key Press", "Input Device", "Keyboard", iconSource: null, description: "Press a key such as Enter", tags: new string[] { "KeyPress" })]
    public class KeyPressActorComponent : InputDeviceActor
    {
        private readonly ILogger logger = Log.ForContext<KeyPressActorComponent>();

        /// <summary>
        /// Indicates whether to simulate a key press, key down or key up
        /// </summary>       
        [DataMember]
        [Display(Name = "Mode", GroupName = "Configuration", Order = 10, Description = "Indicates whether to simulate a key press, key down or key up")]
        public PressMode KeyPressMode { get; set; } = PressMode.KeyPress;
       

        /// <summary>
        /// KeySequence that needs to be simulated
        /// </summary>
        string keySequence = string.Empty;
        [DataMember]
        [Display(Name = "Keys", GroupName = "Configuration", Order = 20, Description = "KeySequence that needs to be simulated")]
        public string KeySequence
        {
            get => keySequence;
            set
            {
                keySequence = value;            
                ValidateComponent();
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public KeyPressActorComponent() : base("Key Press", "KeyPress")
        {

        }

        /// <summary>
        /// Simulate key press, key down or key up for configured key sequence
        /// </summary>
        public override async Task ActAsync()
        {
            var syntheticKeyboard = GetKeyboard();
            var keysToPress = syntheticKeyboard.GetSynthethicKeyCodes(this.KeySequence);
            switch (this.KeyPressMode)
            {
                case PressMode.KeyPress:
                    foreach (var key in keysToPress)
                    {
                        syntheticKeyboard.KeyPress(key);
                    }
                    break;
                case PressMode.KeyDown:
                    foreach (var key in keysToPress)
                    {
                        syntheticKeyboard.KeyDown(key);
                    }
                    break;
                case PressMode.KeyUp:
                    foreach (var key in keysToPress)
                    {
                        syntheticKeyboard.KeyUp(key);
                    }
                    break;
            }
            logger.Information($"{KeyPressMode} performed for sequence {keySequence}");
            await Task.CompletedTask;
        }

        /// <summary>
        /// Check if component is correctly configured
        /// </summary>
        /// <returns></returns>
        public override bool ValidateComponent()
        {
            IsValid = true;
            if (string.IsNullOrEmpty(this.keySequence))
            {
                IsValid = false;
            }
            return IsValid && base.ValidateComponent();
        }

        public override string ToString()
        {
            return "Key Press Actor";
        }
    }
}