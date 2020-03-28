using System.ComponentModel;

namespace Pixel.Automation.Core.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ProjectType
    {   
        [Description("Process Automation")]
        ProcessAutomation,
        [Description("Test Automation")]
        TestAutomation
    }
}
