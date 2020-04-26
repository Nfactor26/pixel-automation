using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    [Serializable]
    public class PrefabDescription : NotifyPropertyChanged, ICloneable
    {
        [DataMember]
        [Browsable(false)]
        public string ApplicationId { get; set; }

        [DataMember]
        [Browsable(false)]
        public string PrefabId { get; set; }

        /// <summary>
        /// Display name that should be visible in Prefab Repository
        /// </summary>
        [DataMember]
        public string PrefabName { get; set; }

        /// <summary>
        /// NameSpace for generated models. NameSpace must be unique
        /// </summary>
        [DataMember]
        public string NameSpace { get; set; }

        /// <summary>
        /// Get all the versions created for this Prefab
        /// </summary>
        [DataMember(IsRequired = true)]
        public List<PrefabVersion> AvailableVersions { get; set; } = new List<PrefabVersion>();
        
        /// <summary>
        /// Get all the versions that are deployed. Deployed Prefabs can be used in an automation.
        /// </summary>
        public IEnumerable<PrefabVersion> DeployedVersions { get => AvailableVersions.Where(a => a.IsDeployed).ToList(); }

        public PrefabVersion ActiveVersion 
        {
            get => this.AvailableVersions.FirstOrDefault(a => a.IsActive);
        }  

        [DataMember]
        public string Description { get; set; }

        string groupName = "Default";
        [DataMember]
        public string GroupName
        {
            get
            {
                return groupName;
            }
            set
            {
                groupName = value;
                OnPropertyChanged();
            }
        }

        public Interfaces.IComponent PrefabRoot { get; set; }


        public PrefabDescription()
        {

        }

        public PrefabDescription(Interfaces.IComponent prefabRoot)
        {
            this.PrefabRoot = prefabRoot;           
        }

        public object Clone()
        {
            return new PrefabDescription()
            {
                PrefabName = this.PrefabName,             
                PrefabId = Guid.NewGuid().ToString(),             
                GroupName = this.GroupName,
                ApplicationId = this.ApplicationId,
                PrefabRoot = (PrefabRoot as ICloneable).Clone() as Interfaces.IComponent
            };
        }

        public override bool Equals(object obj)
        {
            if(obj is PrefabDescription otherPrefab)
            {
                return this.ApplicationId.Equals(otherPrefab.ApplicationId) && this.PrefabId.Equals(otherPrefab.PrefabId);
            }
            return false;
        }
    }
}
