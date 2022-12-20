using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    /// <summary>
    /// Tracks usage of a prefab across fixtures and test cases
    /// </summary>
    [DataContract]
    public class PrefabUsage
    {
        /// <summary>
        /// Identifier of Prefab
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        public string PrefabId { get; set; }

        /// <summary>
        /// Reference count
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        public int Count { get; set; }
    }
}
