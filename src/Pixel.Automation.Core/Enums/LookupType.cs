using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Enums
{
    [DataContract]
    [Serializable]
    public enum LookupType
    {
        Default,   // Default controls are always looked up in default search root of application
        Relative   // Control is looked up relative to it's parent control.
    }   
}
