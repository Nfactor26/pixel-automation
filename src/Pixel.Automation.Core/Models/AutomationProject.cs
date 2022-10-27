using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    /// <summary>
    /// Project details for the automation project
    /// </summary>
    [Serializable]
    [DataContract]
    public class AutomationProject
    {
        /// <summary>
        /// Unique Identifier of the automation project
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        public string ProjectId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Display Name of the automation project
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// NameSpace for generated models. NameSpace must be unique
        /// </summary>
        [DataMember(IsRequired = true, Order = 40)]
        public string Namespace { get; set; }
      
        [DataMember(IsRequired = true, Order = 50)]
        public List<ProjectVersion> AvailableVersions { get; } = new List<ProjectVersion>();
       
        /// <summary>
        /// Get all the versions that are active.
        /// </summary>
        public IEnumerable<ProjectVersion> ActiveVersions { get => AvailableVersions.Where(a => a.IsActive).ToList(); }

        /// <summary>
        /// Get all the versions that are published.
        /// </summary>
        public IEnumerable<ProjectVersion> PublishedVersions { get => AvailableVersions.Where(a => a.IsPublished).ToList(); }
       
        /// <summary>
        /// Latest Active version of the project
        /// </summary>
        public ProjectVersion LatestActiveVersion
        {
            get => this.ActiveVersions.OrderBy(a => a.Version).LastOrDefault();
        }

        /// <summary>
        /// constructor
        /// </summary>
        public AutomationProject()
        {
           
        }
    }
}
