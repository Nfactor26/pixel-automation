using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Application
{
    /// <summary>
    /// Windows based application that supports automation using UIA framework
    /// </summary>
    [DataContract]
    [Serializable]
    public class WindowsApplication : Application
    {
        /// <summary>
        /// Path of the executable file
        /// </summary>
        [DataMember(IsRequired = true)]
        public string ExecutablePath { get; set; }
    }
}
