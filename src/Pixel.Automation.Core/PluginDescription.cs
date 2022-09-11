using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core
{
    public enum PluginType
    {
        PlatformFeature,
        Component,
        Scrapper
    }

    [DataContract]
    public class PluginDescription
    {
        /// <summary>
        /// Name of the Plugin dll without extension
        /// </summary>
        [DataMember(Order = 10)]
        public string Name { get; set; }        

        /// <summary>
        /// Type of the Plugin
        /// </summary>
        [DataMember(Order = 20)]
        public PluginType Type { get; set; }

        /// <summary>
        /// Supported platforms for Plugin
        /// </summary>
        [DataMember(Order = 30)]
        public IEnumerable<string> SupportedPlatforms { get; set; }
        
    }
}
