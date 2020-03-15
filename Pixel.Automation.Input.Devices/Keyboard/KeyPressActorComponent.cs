using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Input.Devices
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Key Press", "Input Device", "Keyboard", iconSource: null, description: "Press a key such as Enter", tags: new string[] { "KeyPress" })]

    public class KeyPressActorComponent : InputSimulatorBase
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
                OnPropertyChanged("KeyPressMode");
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
                OnPropertyChanged("KeySequence");
            }
        }


        public KeyPressActorComponent() : base("Key Press", "KeyPress")
        {

        }

        public override void Act()
        {
            var syntheticKeyboard = GetKeyboard();
            var keysToPress = syntheticKeyboard.GetSynthethicKeyCodes(this.keySequence);
            switch (this.pressMode)
            {
                case PressMode.KeyPress:
                    foreach (var key in keysToPress)
                        syntheticKeyboard.KeyPress(key);
                    break;
                case PressMode.KeyDown:
                    foreach (var key in keysToPress)
                        syntheticKeyboard.KeyDown(key);
                    break;
                case PressMode.KeyUp:
                    foreach (var key in keysToPress)
                        syntheticKeyboard.KeyUp(key);
                    break;
            }

        }        
    }
}