using System;
using System.ComponentModel;

namespace Pixel.Automation.Core.Devices
{
    [Serializable]
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum PressMode
    {
        [Description("Key Press")]
        KeyPress,
        [Description("Key Down")]
        KeyDown,
        [Description("Key Up")]
        KeyUp
    }
}
