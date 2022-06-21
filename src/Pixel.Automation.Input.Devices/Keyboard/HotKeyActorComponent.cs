using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices
{
    /// <summary>
    /// Use <see cref="HotKeyActorComponent"/> to simulate pressing hotkey combinations such as Ctrl + A , Alt + E, etc.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Hot Key", "Input Device", "Keyboard", iconSource: null, description: "Press a key comibnation shuch as ctrl+c,etc.", tags: new string[] { "HotKeys" })]
    public class HotKeyActorComponent : InputDeviceActor
    {
        private readonly ILogger logger = Log.ForContext<HotKeyActorComponent>();

        string keySequence = "Ctrl + C";
        [DataMember]
        [Display(Name = "Hot Key", GroupName = "Configuration", Order = 10)]
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
        public HotKeyActorComponent() : base("Hot Key", "HotKey")
        {

        }

        /// <summary>
        /// Simulate pressing configured HotKeys
        /// </summary>
        public override async Task ActAsync()
        {
            var synthethicKeyboard = GetKeyboard();
            var syntheticKeySequence = synthethicKeyboard.GetSynthethicKeyCodes(KeySequence);
            var modifiers = syntheticKeySequence.TakeWhile(s => synthethicKeyboard.IsModifierKey(s));
            var keys = syntheticKeySequence.SkipWhile(s => synthethicKeyboard.IsModifierKey(s));
            synthethicKeyboard.ModifiedKeyStroke(modifiers, keys);
            logger.Information($"Hot keys : {KeySequence} were pressed.");
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
            return "Hot Key Actor";
        }
    }
}
