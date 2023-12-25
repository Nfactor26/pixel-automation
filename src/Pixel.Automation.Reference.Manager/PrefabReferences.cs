using Dawn;
using Pixel.Automation.Core.Models;
using System.Runtime.Serialization;

namespace Pixel.Automation.Reference.Manager
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
        public List<PrefabReference> References { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="references"></param>
        public PrefabReferences(List<PrefabReference> references)
        {
            Guard.Argument(references, nameof(references)).NotNull();
            References = references;
        }
    

        /// <summary>
        /// Adds reference to a prefab for AutomationProcess
        /// </summary>
        /// <param name="prefabReference"></param>
        public void AddPrefabReference(PrefabReference prefabReference)
        {
            if (!References.Any(a => a.Equals(prefabReference)))
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

        public bool HasReference(PrefabReference prefabReference)
        {
            return References.Any(a => a.Equals(prefabReference));
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
            var reference = References.FirstOrDefault(a => a.PrefabId.Equals(prefabId));
            return reference ?? throw new ArgumentException($"No reference exists for Prefab with Id {prefabId}");
        }

        /// <summary>
        /// Get the version of Prefab in use by Automation process
        /// </summary>
        /// <param name="prefabProject"></param>
        /// <returns></returns>
        public VersionInfo GetPrefabVersionInUse(PrefabProject prefabProject)
        {
            var reference = References.FirstOrDefault(a => a.Equals(prefabProject));
            if (reference != null)
            {
                return reference.Version;
            }
            throw new InvalidOperationException($"Failed to get version in use. No reference exists for Prefab with Id : {prefabProject.ProjectId}");
        }
    }

}
