using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [Serializable]
    [DataContract]
    public class AutomationProject
    {
        [DataMember(IsRequired = true, Order = 10)]
        public string ProjectId { get; set; }

       
        [DataMember(IsRequired = true, Order = 20)]
        public string Name { get; set; }
       

        [DataMember(Order = 30)]
        public ProjectType ProjectType { get; set; }

        [DataMember(IsRequired = true, Order = 40)]
        public DateTime LastOpened { get; set; }

        [DataMember(IsRequired = true)]
        public List<ProjectVersion> AvailableVersions { get; set; } = new List<ProjectVersion>();

        /// <summary>
        /// Get all the versions that are deployed. Deployed Prefabs can be used in an automation.
        /// </summary>
        public IEnumerable<ProjectVersion> DeployedVersions { get => AvailableVersions.Where(a => a.IsDeployed).ToList(); }

        public ProjectVersion ActiveVersion
        {
            get => this.AvailableVersions.FirstOrDefault(a => a.IsActive);
        }

        public AutomationProject()
        {
           
        }         
    }
}
