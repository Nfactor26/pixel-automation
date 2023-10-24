using Pixel.Automation.Core;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.UIA.Components.Enums;

[DataContract]
[Serializable]
[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum SelectMode
{
    [Description("Select")]
    Select,
    [Description("Add to selection")]
    AddToSelection,
    [Description("Remove from selection")]
    RemoveFromSelection
}
