using System;
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

        [DataMember(IsRequired = false)]
        public string DataModelAssembly { get; set; }

        [DataMember(IsRequired = false)]
        public string Description { get; set; }

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
        public ProjectVersion() : this(new Version(1, 0))
        {
        }

        public ProjectVersion(Version version)
        {
            this.Version = version;
        }

        public ProjectVersion(string version) : this(new Version(version))
        {

        }
    }

    [DataContract]
    [Serializable]
    public class PrefabVersion : VersionInfo
    {
        public PrefabVersion() : this(new Version(1, 0))
        {
        }

        public PrefabVersion(Version version)
        {
            this.Version = version;
        }

        public PrefabVersion(string version) : this(new Version(version))
        {

        }
    }
}
