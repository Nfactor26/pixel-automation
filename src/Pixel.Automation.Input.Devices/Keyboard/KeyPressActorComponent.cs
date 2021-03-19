using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Input.Devices
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Key Press", "Input Device", "Keyboard", iconSource: null, description: "Press a key such as Enter", tags: new string[] { "KeyPress" })]

    public class KeyPressActorComponent : DeviceInputActor
    {
        PressMode pressMode;
        [DataMember]
        [Display(Name = "Mode", GroupName = "Key Configuration")]
        public PressMode KeyPressMode
        {
            get
            {
                return pressMode;
            }
            set
            {
                pressMode = value;
                OnPropertyChanged();
            }
        }


        string keySequence = string.Empty;
        [DataMember]
        [Display(Name = "Keys", GroupName = "Key Configuration")]
        public string KeySequence
        {
            get => keySequence;
            set
            {
                keySequence = value;               
                OnPropertyChanged();
            }
        }


        public KeyPressActorComponent() : base("Key Press", "KeyPress")
        {

        }

        public override void Act()
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