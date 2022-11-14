using Dawn;
using Pixel.Automation.Core.Models;
using System.Runtime.Serialization;

namespace Pixel.Scripting.Reference.Manager
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
        public List<ControlReference> References { get; private set; }

        public ControlReferences(List<ControlReference> references)
        {
            Guard.Argument(references, nameof(references)).NotNull();
            this.References = references;
        }

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

}
