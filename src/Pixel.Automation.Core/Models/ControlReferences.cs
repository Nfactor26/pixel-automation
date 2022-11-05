using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    /// <summary>
    /// Maps Prefabs to TestCases that have a reference to this Prefab
    /// </summary>
    [DataContract]
    public class ControlReference
    {
        /// <summary>
        /// ApplicationId to which Prefab belongs
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        public string ApplicationId { get; set; }

        /// <summary>
        /// Identifier of Prefab
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        public string ControlId { get; set; }

        /// <summary>
        /// Version of control in use
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false, Order = 30)]
        public Version Version { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ControlReference()
        {

        }

        /// <summary>
        /// constructor
        /// </summary>
        public ControlReference(string applicationId, string controlId)
        {
            this.ApplicationId = applicationId;
            this.ControlId = controlId;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public ControlReference(string applicationId, string controlId, Version version) : this(applicationId, controlId)
        {
            this.Version = version;
        }

        public override bool Equals(object obj)
        {
            if (obj is ControlReference cr)
            {
                return cr.ApplicationId.Equals(this.ApplicationId) && cr.ControlId.Equals(this.ControlId);
            }            
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ApplicationId, ControlId);
        }
    }
}
