using Pixel.Automation.Core.Interfaces;
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
    public class AutomationProject : IProject
    {   
        /// <inheritdoc/>       
        [DataMember(IsRequired = true, Order = 10)]
        public string ProjectId { get; set; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>  
        [DataMember(IsRequired = true, Order = 20)]
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc/>  
        [DataMember(IsRequired = true, Order = 40)]
        public string Namespace { get; set; }

        /// <inheritdoc/>  
        [DataMember(IsRequired = true, Order = 50)]
        public List<VersionInfo> AvailableVersions { get; } = new ();

        /// <inheritdoc/>  
        public IEnumerable<VersionInfo> ActiveVersions { get => AvailableVersions.Where(a => a.IsActive).ToList(); }

        /// <inheritdoc/>  
        public IEnumerable<VersionInfo> PublishedVersions { get => AvailableVersions.Where(a => a.IsPublished).ToList(); }

        /// <inheritdoc/>  
        public VersionInfo LatestActiveVersion
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
