using System.Runtime.Serialization;

namespace Pixel.Automation.Editor.Core
{
    [DataContract]
    public class UserSettings
    {
        [DataMember]
        public string Theme { get; set; }

        [DataMember]
        public string Accent { get; set; }

        [DataMember]
        public bool ShowConsoleWindow { get; set; }
    }
}
