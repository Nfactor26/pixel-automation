using System.ComponentModel;

namespace Pixel.Automation.Core.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Priority
    {
        [Description("Default")]
        Default,
        [Description("Low")]
        Low,
        [Description("Low")]
        Medium,
        [Description("High")]
        High       
    }
}
