using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    /// <summary>
    /// Tracks usage of a control across fixtures and test cases
    /// </summary>
    [DataContract]
    public class ControlUsage
    {
        /// <summary>
        /// Identifier of Control
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        public string ControlId { get; set; }

        /// <summary>
        /// Reference count
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        public int Count { get; set; }
    }
}
