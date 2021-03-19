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
        /// Identifier of the AutomationProject for which mapping is stored
        /// </summary>
        [DataMember]
        public string ProjectId { get; set; }

        /// <summary>
        /// Collection of PrefabReference i.e. Prefabs in use by AutomationProject
        /// </summary>
        [DataMember]
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
        /// <param name="prefabDescription"></param>
        /// <returns></returns>
        public bool HasReference(PrefabDescription prefabDescription)
        {
            return References.Any(a => a.Equals(prefabDescription));
        }

        /// <summary>
        /// Get the PrefabReference for a given prefabId
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        public PrefabReference GetPrefabReference(string prefabId)
        {
            var reference =  References.FirstOrDefault(a => a.PrefabDetails.PrefabId.Equals(prefabId));
            return reference ?? throw new ArgumentException($"No reference exists for Prefab with Id {prefabId}");
        }

        /// <summary>
        /// Get the version of Prefab in use by Automation process
        /// </summary>
        /// <param name="prefabDescription"></param>
        /// <returns></returns>
        public PrefabVersion GetPrefabVersionInUse(PrefabDescription prefabDescription)
        {
           var reference  =  References.FirstOrDefault(a => a.Equals(prefabDescription));
            if(reference != null)
            {
                return reference.PrefabDetails.Version;
            }
            throw new InvalidOperationException($"Failed to get version in use. No reference exists for Prefab with Id : {prefabDescription.PrefabId}");
        }
    }


    /// <summary>
    /// Maps Prefabs to TestCases that have a reference to this Prefab
    /// </summary>
    [DataContract]
    public class PrefabReference
    {
       [DataMember]
       public ReferencedPrefabDetails PrefabDetails { get; set; }

        /// <summary>
        /// Collection of TestCase Identifiers which have a reference to this Prefab
        /// </summary>
        [DataMember]
        public List<ReferringEntityDetails> ReferringEntities { get; set; } = new List<ReferringEntityDetails>();


        public void AddReference(ReferringEntityDetails referringEntity)
        {
            if(!this.ReferringEntities.Contains(referringEntity))
            {
                this.ReferringEntities.Add(referringEntity);
            }
        }

        public void RemoveReference(ReferringEntityDetails referringEntity)
        {
            if (this.ReferringEntities.Contains(referringEntity))
            {
                this.ReferringEntities.Remove(referringEntity);
            }
        }

        public override bool Equals(object obj)
        {
            if(obj is PrefabReference pr)
            {
                return pr.PrefabDetails.Equals(this.PrefabDetails);           
            }
            if(obj is PrefabDescription pd)
            {
                return pd.ApplicationId.Equals(this.PrefabDetails.ApplicationId) && pd.PrefabId.Equals(this.PrefabDetails.PrefabId);
            }
            return false;
        }

    }

    [DataContract]
    public class ReferencedPrefabDetails
    {
        /// <summary>
        /// ApplicationId to which Prefab belongs
        /// </summary>
        [DataMember]
        public string ApplicationId { get; set; }

        /// <summary>
        /// Identifier of Prefab
        /// </summary>
        [DataMember]
        public string PrefabId { get; set; }

        /// <summary>
        /// Version of Prefab in use
        /// </summary>
        [DataMember]
        public PrefabVersion Version { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ReferencedPrefabDetails r)
            {
                return r.ApplicationId.Equals(this.ApplicationId) && r.PrefabId.Equals(this.PrefabId) && r.Version.Equals(this.Version);
            }
            return false;
        }
    }

    [DataContract]
    public class ReferringEntityDetails
    {
        /// <summary>
        /// Id of the test case to which PrefabEntity belongs. This can be empty when PrefabEntity is added outside test case.
        /// </summary>
        [DataMember(IsRequired = false)]
        public string TestCaseId { get; set; }

        /// <summary>
        /// Id of the PrefabEntity added to process
        /// </summary>
        [DataMember]
        public string EntityId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ReferringEntityDetails r)
            {
                return r.EntityId.Equals(this.EntityId) && (( r.TestCaseId == null && this.TestCaseId == null) || r.TestCaseId.Equals(this.TestCaseId));
            }
            return false;
        }

    }
}
