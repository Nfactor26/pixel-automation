using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Enums
{
    [DataContract]
    [Serializable]
    public enum LookupMode
    {
        FindSingle, ///Find only match
        FindAll ///Find all matching elements and then use filter by index or custom filter to get target match
    }
}
