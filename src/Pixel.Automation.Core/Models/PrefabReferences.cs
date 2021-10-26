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
    public class PrefabReferences
    {
        /// <summary>
        /// Collection of PrefabReference i.e. Prefabs in use by AutomationProject
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        public List<PrefabReference> References { get; set; } = new List<PrefabReference>();

        /// <summary>
        /// Adds reference to a prefab for AutomationProcess
        /// </summary>
        /// <param name="prefabReference"></param>
        public void AddPrefabReference(PrefabReference prefabReference)
        {
            if(!References.Any(a => a.Equals(prefabReference)))
            {
                References.Add(prefabReference);               
            }
        }

        /// <summary>
        /// Remove reference to a prefab for a given automation process
        /// </summary>
        /// <param name="prefabRerence"></param>
        public void RemovePrefabReference(PrefabReference prefabRerence)
        {
            References.RemoveAll(a => a.Equals(prefabRerence));
        }           

        /// <summary>
        /// Check if the automation process uses a prefab whose details are provided
        /// </summary>
        /// <param name="prefabProject"></param>
        /// <returns></returns>
        public bool HasReference(PrefabProject prefabProject)
        {
            return References.Any(a => a.Equals(prefabProject));
        }

        /// <summary>
        /// Check if the automation process uses a prefab whose details are provided
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        public bool HasReference(string prefabId)
        {
            return References.Any(a => a.PrefabId.Equals(prefabId));
        }

        /// <summary>
        /// Get the PrefabReference for a given prefabId
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        public PrefabReference GetPrefabReference(string prefabId)
        {
            var reference =  References.FirstOrDefault(a => a.PrefabId.Equals(prefabId));
            return reference ?? throw new ArgumentException($"No reference exists for Prefab with Id {prefabId}");
        }

        /// <summary>
        /// Get the version of Prefab in use by Automation process
        /// </summary>
        /// <param name="prefabProject"></param>
        /// <returns></returns>
        public PrefabVersion GetPrefabVersionInUse(PrefabProject prefabProject)
        {
            var reference = References.FirstOrDefault(a => a.Equals(prefabProject));
            if (reference != null)
            {
                return reference.Version;
            }
            throw new InvalidOperationException($"Failed to get version in use. No reference exists for Prefab with Id : {prefabProject.PrefabId}");
        }
    }


    /// <summary>
    /// Maps Prefabs to TestCases that have a reference to this Prefab
    /// </summary>
    [DataContract]
    public class PrefabReference
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
        public string PrefabId { get; set; }

        /// <summary>
        /// Version of Prefab in use
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false , Order = 30)]
        public PrefabVersion Version { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PrefabReference()
        {

        }

        /// <summary>
        /// constructor
        /// </summary>
        public PrefabReference(string applicationId, string prefabId)
        {
            this.ApplicationId = applicationId;
            this.PrefabId = prefabId;          
        }

        /// <summary>
        /// constructor
        /// </summary>
        public PrefabReference(string applicationId, string prefabId, PrefabVersion version) : this(applicationId, prefabId)
        {           
            this.Version = version;
        }

        public override bool Equals(object obj)
        {
            if(obj is PrefabReference pr)
            {
                return pr.ApplicationId.Equals(this.ApplicationId) && pr.PrefabId.Equals(this.PrefabId);
            }
            if (obj is PrefabProject pd)
            {
                return pd.ApplicationId.Equals(this.ApplicationId) && pd.PrefabId.Equals(this.PrefabId);
            }
            return false;
        }
    }
}
