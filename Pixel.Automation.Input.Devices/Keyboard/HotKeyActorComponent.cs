using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Input.Devices
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Hot Key", "Input Device", "Keyboard", iconSource: null, description: "Press a key comibnation shuch as ctrl+c,etc.", tags: new string[] { "HotKeys" })]

    public class HotKeyActorComponent : DeviceInputActor
    {
        string keySequence = "Ctrl + C";
        [DataMember]
        [Display(Name = "Hot Key", GroupName = "Key Configuration")]          
        public string KeySequence
        {
            get => keySequence;
            set
            {
                keySequence = value;                           
                OnPropertyChanged();
            }
        }


        public HotKeyActorComponent() : base("Hot Key", "HotKey")
        {
          
        }

        public override void Act()
        {
            var synthethicKeyboard = GetKeyboard();
            var syntheticKeySequence = synthethicKeyboard.GetSynthethicKeyCodes(KeySequence);
            var modifiers = syntheticKeySequence.TakeWhile(s => synthethicKeyboard.IsModifierKey(s));
            var keys = syntheticKeySequence.SkipWhile(s => synthethicKeyboard.IsModifierKey(s));
            synthethicKeyboard.ModifiedKeyStroke(modifiers, keys);          
        }

        public override bool ValidateComponent()
        {
            if (string.IsNullOrEmpty(this.keySequence))
            {
                IsValid = false;
            }
            return IsValid && base.ValidateComponent();
        }
    }
}
