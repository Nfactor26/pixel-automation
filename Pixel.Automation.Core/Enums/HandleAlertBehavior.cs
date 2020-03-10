using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Enums
{
    [DataContract]
    [Serializable]
    public enum HandleAlertBehavior
    {
        Accept,
        Dismiss
    }
}
