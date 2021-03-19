using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Enums
{
    [DataContract]
    [Serializable]
    public enum SearchScope
    {
        Children = 1,
        Descendants = 2,
        Sibling = 4,
        Ancestor = 8
    }
}
