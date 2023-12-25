using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{

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
        public VersionInfo Version { get; set; }

        /// <summary>
        /// Input mapping script file for the prefab
        /// </summary>
        public string InputMappingScriptFile { get; set; }
       
        /// <summary>
        /// Output mapping script file for the prefab
        /// </summary>
        public string OutputMappingScriptFile { get; set; }       

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
        public PrefabReference(string applicationId, string prefabId, VersionInfo version) : this(applicationId, prefabId)
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
                return pd.ApplicationId.Equals(this.ApplicationId) && pd.ProjectId.Equals(this.PrefabId);
            }
            return false;
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(ApplicationId, PrefabId);
        }
    }
}
