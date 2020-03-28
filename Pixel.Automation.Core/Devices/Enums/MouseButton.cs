using System;
using System.ComponentModel;

namespace Pixel.Automation.Core.Devices
{
    [Serializable]
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum MouseButton
    {
        [Description("Left Button")]
        LeftButton = 0,
        [Description("Middle Button")]
        MiddleButton = 1,
        [Description("Right Button")]
        RightButton = 2
    }
}
