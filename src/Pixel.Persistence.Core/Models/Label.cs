using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class Label
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}
