using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    [Serializable]
    public class PrefabProject : ICloneable, IProject
    {
        [DataMember(IsRequired = true, Order = 10)]
        [Browsable(false)]
        public string ApplicationId { get; set; }

        [DataMember(IsRequired = true, Order = 20)]
        [Browsable(false)]
        public string ProjectId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Display name that should be visible in Prefab Repository
        /// </summary>
        [DataMember(IsRequired = true, Order = 30)]
        public string Name { get; set; }

        /// <summary>
        /// NameSpace for generated models. NameSpace must be unique
        /// </summary>
        [DataMember(IsRequired = true, Order = 40)]
        public string Namespace { get; set; }

        /// <summary>
        /// Get all the versions created for this Prefab
        /// </summary>
        [DataMember(IsRequired = true, Order = 50)]
        public List<VersionInfo> AvailableVersions { get; set; } = new List<VersionInfo>();     

        /// <summary>
        /// Description of the prefab
        /// </summary>
        [DataMember(Order = 60)]
        public string Description { get; set; }

        /// <summary>
        /// Group name used for grouping on UI
        /// </summary>
        [DataMember(Order = 70)]
        public string GroupName { get; set; } = "Default";

        /// <summary>
        /// Indicates if Prefab project is marked deleted.
        /// </summary>
        [DataMember(Order = 1000)]
        public bool IsDeleted { get; set; }

        /// <inheritdoc/>  
        public IEnumerable<VersionInfo> ActiveVersions { get => AvailableVersions.Where(a => a.IsActive).ToList(); }

        /// <inheritdoc/>  
        public IEnumerable<VersionInfo> PublishedVersions { get => AvailableVersions.Where(a => a.IsPublished).ToList(); }

        /// <inheritdoc/>  
        public VersionInfo LatestActiveVersion
        {
            get => this.ActiveVersions.OrderBy(a => a.Version).LastOrDefault();
        }

        [JsonIgnore]
        public Interfaces.IComponent PrefabRoot { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public PrefabProject()
        {

        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="prefabRoot"></param>
        public PrefabProject(Interfaces.IComponent prefabRoot)
        {
            this.PrefabRoot = prefabRoot;           
        }
      
        ///</inheritdoc>
        public object Clone()
        {
            return new PrefabProject()
            {
                Name = this.Name,             
                ProjectId = Guid.NewGuid().ToString(),             
                GroupName = this.GroupName,
                ApplicationId = this.ApplicationId,
                Description = this.Description,
                PrefabRoot = (PrefabRoot as ICloneable).Clone() as Interfaces.IComponent
            };
        }

        ///</inheritdoc>
        public override bool Equals(object obj)
        {
            if(obj is PrefabProject otherPrefab)
            {
                return this.ApplicationId.Equals(otherPrefab.ApplicationId) && this.ProjectId.Equals(otherPrefab.ProjectId);
            }
            return false;
        }

        ///</inheritdoc>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.ProjectId, this.ApplicationId);
        }
    }
}
