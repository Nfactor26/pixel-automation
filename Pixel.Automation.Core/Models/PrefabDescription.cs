using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    [Serializable]
    public class PrefabDescription : NotifyPropertyChanged, ICloneable
    {
        [DataMember]
        public string ApplicationId { get; set; }

        [DataMember]
        public string PrefabId { get; set; }

        /// <summary>
        /// Display name that should be visible in Prefab Repository
        /// </summary>
        [DataMember]
        public string PrefabName { get; set; }

        [DataMember(IsRequired = true)]
        public List<Version> AvailableVersions { get; set; }
    
        /// <summary>
        /// Version of the Prefab
        /// </summary>
        [DataMember(IsRequired = true)]
        public Version ActiveVersion { get; set; }

        [DataMember(IsRequired = false)]
        public Version DeployedVersion { get; set; }

        /// <summary>
        /// Name of the data model assembly that was generated while conjuring this prefab
        /// </summary>
        [DataMember]
        public string AssemblyName { get; set; }

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

        public void SetActiveVersion(Version version)
        {
            this.ActiveVersion = version;
            if (!this.AvailableVersions.Contains(version))
            {
                this.AvailableVersions.Add(version);
            }         
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
    }
}
