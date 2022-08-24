using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    /// <summary>
    /// Maintains a mapping of Prefabs (along with PrefabEntity identifer) used in a AutomationProcess.
    /// </summary>
    [DataContract]
    public class ControlReferences
    {
        /// <summary>
        /// Collection of ControlReference i.e. Controls in use by AutomationProject
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        public List<ControlReference> References { get; set; } = new ();

        /// <summary>
        /// Adds reference to a control for AutomationProcess
        /// </summary>
        /// <param name="controlReference"></param>
        public void AddControlReference(ControlReference controlReference)
        {
            if (!References.Any(a => a.Equals(controlReference)))
            {
                References.Add(controlReference);
            }
        }

        /// <summary>
        /// Remove reference to a Control for a given automation process
        /// </summary>
        /// <param name="prefabRerence"></param>
        public void RemoveControlReference(ControlReference controlReference)
        {
            References.RemoveAll(a => a.Equals(controlReference));
        }

        /// <summary>
        /// Check if the automation process uses a control whose details are provided
        /// </summary>
        /// <param name="prefabProject"></param>
        /// <returns></returns>
        public bool HasReference(ControlReference controlReference)
        {
            return References.Any(a => a.Equals(controlReference));
        }

        /// <summary>
        /// Check if the automation process uses a control whose details are provided
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        public bool HasReference(string controlId)
        {
            return References.Any(a => a.ControlId.Equals(controlId));
        }

        /// <summary>
        /// Get the ControlReference for a given controlId
        /// </summary>     
        /// <param name="controlId"></param>
        /// <returns></returns>
        public ControlReference GetControlReference(string controlId)
        {
            var reference = References.FirstOrDefault(a => a.ControlId.Equals(controlId));
            return reference ?? throw new ArgumentException($"No reference exists for Control with Id {controlId}");
        }

        /// <summary>
        /// Get the version of Control in use by Automation process
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        public Version GetControlVersionInUse(string applicationId, string controlId)
        {
            var reference = References.FirstOrDefault(a => a.ApplicationId.Equals(applicationId) && a.ControlId.Equals(controlId));
            if (reference != null)
            {
                return reference.Version;
            }
            throw new InvalidOperationException($"Failed to get version in use. No reference exists for Control with Id : {controlId}");
        }
    }

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
        /// Version of Prefab in use
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
    }
}
