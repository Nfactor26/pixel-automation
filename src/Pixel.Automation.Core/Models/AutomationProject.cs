﻿using System;
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
        [DataMember(IsRequired = true, Order = 10)]
        public string ProjectId { get; set; } = Guid.NewGuid().ToString();

        [DataMember(IsRequired = true, Order = 20)]
        public string Name { get; set; } = string.Empty;

        [DataMember(IsRequired = true, Order = 40)]
        public DateTime LastOpened { get; set; } = DateTime.Now;

        [DataMember(IsRequired = true, Order = 50)]
        public List<ProjectVersion> AvailableVersions { get; } = new List<ProjectVersion>();

        /// <summary>
        /// Get all the versions that are deployed.
        /// </summary>
        public IEnumerable<ProjectVersion> DeployedVersions { get => AvailableVersions.Where(a => a.IsDeployed).ToList(); }

        public ProjectVersion ActiveVersion
        {
            get => this.AvailableVersions.FirstOrDefault(a => a.IsActive);
        }

        /// <summary>
        /// constructor
        /// </summary>
        public AutomationProject()
        {
           
        }

        /// <summary>
        /// Get the name of the project. Spaces are replaced by _ .
        /// </summary>
        /// <returns></returns>
        public string GetProjectName()
        {
            return this.Name.Trim().Replace(' ','_');
        }
    }
}
