﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    [Serializable]
    public class PrefabProject : NotifyPropertyChanged, ICloneable
    {
        [DataMember(IsRequired = true, Order = 10)]
        [Browsable(false)]
        public string ApplicationId { get; set; }

        [DataMember(IsRequired = true, Order = 20)]
        [Browsable(false)]
        public string PrefabId { get; set; }

        /// <summary>
        /// Display name that should be visible in Prefab Repository
        /// </summary>
        [DataMember(IsRequired = true, Order = 30)]
        public string PrefabName { get; set; }

        /// <summary>
        /// NameSpace for generated models. NameSpace must be unique
        /// </summary>
        [DataMember(IsRequired = true, Order = 40)]
        public string NameSpace { get; set; }

        /// <summary>
        /// Get all the versions created for this Prefab
        /// </summary>
        [DataMember(IsRequired = true, Order = 50)]
        public List<PrefabVersion> AvailableVersions { get; set; } = new List<PrefabVersion>();
        
        /// <summary>
        /// Get all the versions that are deployed. Deployed Prefabs can be used in an automation.
        /// </summary>
        public IEnumerable<PrefabVersion> DeployedVersions { get => AvailableVersions.Where(a => a.IsDeployed).ToList(); }

        public PrefabVersion ActiveVersion 
        {
            get => this.AvailableVersions.FirstOrDefault(a => a.IsActive);
        }

        [DataMember(Order = 60)]
        public string Description { get; set; }

        string groupName = "Default";
        [DataMember(Order = 70)]
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


        public PrefabProject()
        {

        }

        public PrefabProject(Interfaces.IComponent prefabRoot)
        {
            this.PrefabRoot = prefabRoot;           
        }


        public string GetPrefabName()
        {
            return this.PrefabName.Trim().Replace(' ', '_');
        }

        public object Clone()
        {
            return new PrefabProject()
            {
                PrefabName = this.PrefabName,             
                PrefabId = Guid.NewGuid().ToString(),             
                GroupName = this.GroupName,
                ApplicationId = this.ApplicationId,
                Description = this.Description,
                PrefabRoot = (PrefabRoot as ICloneable).Clone() as Interfaces.IComponent
            };
        }

        public override bool Equals(object obj)
        {
            if(obj is PrefabProject otherPrefab)
            {
                return this.ApplicationId.Equals(otherPrefab.ApplicationId) && this.PrefabId.Equals(otherPrefab.PrefabId);
            }
            return false;
        }
    }
}
