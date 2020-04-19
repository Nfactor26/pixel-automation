﻿using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    [Serializable]
    public abstract class VersionInfo
    {
        [DataMember]
        public Version Version { get; set; }

        [DataMember]
        public bool IsDeployed { get; set; } = false;

        [DataMember]
        public bool IsActive { get; set; } = true;

        public override bool Equals(object obj)
        {
            if (obj is VersionInfo other)
            {
                return this.Version.Equals(other.Version);
            }
            return false;
        }

        public override string ToString()
        {
            return this.Version.ToString();
        }
    }

    [DataContract]
    [Serializable]
    public class ProjectVersion : VersionInfo
    {       

        public ProjectVersion()
        {
        }

        public ProjectVersion(Version version)
        {
            this.Version = version;
        }
    }

    [DataContract]
    [Serializable]
    public class PrefabVersion : VersionInfo
    {  

        [DataMember(IsRequired = false)]
        public string PrefabAssembly { get; set; }      

        public PrefabVersion()
        {
        }

        public PrefabVersion(Version version)
        {
            this.Version = version;
        }       
    }
}