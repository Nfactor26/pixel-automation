using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Application
{
    /// <summary>
    /// Browser based application that supports automation using Selenium
    /// </summary>
    [DataContract]
    [Serializable]
    public class WebBrowserApplication : Application
    {
        [DataMember(IsRequired = true)]
        public Uri ApplicationUrl { get; set; }

        [DataMember(IsRequired = true)]
        public Browsers PreferredBrowser { get; set; }
    }
}
