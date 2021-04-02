using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Enums
{
    [DataContract]
    [Serializable]
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AttachMode
    {
        [Description("Attach to executable")]
        AttachToExecutable,
        [Description("Attach to window")]
        AttachToWindow       
    }
}
