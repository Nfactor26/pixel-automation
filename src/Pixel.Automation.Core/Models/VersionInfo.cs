using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    /// <summary>
    /// Captures version details such as version, deployed, etc.
    /// </summary>
    [DataContract]
    [Serializable]
    public abstract class VersionInfo
    {
        /// <summary>
        /// Version
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        public Version Version { get; set; }
      
        /// <summary>
        /// Indicates if the version is active
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Data model assembly name for the automation or prefab project
        /// </summary>
        [DataMember(IsRequired = false, Order = 30)]
        public string DataModelAssembly { get; set; }

        /// <summary>
        /// Description 
        /// </summary>
        [DataMember(IsRequired = false, Order = 40)]
        public string Description { get; set; }

        public bool IsPublished => !this.IsActive;

        ///</inheritdoc>
        public override bool Equals(object obj)
        {
            if (obj is VersionInfo other)
            {
                return this.Version.Equals(other.Version);
            }
            return false;
        }

        ///</inheritdoc>
        public override string ToString()
        {
            return this.Version.ToString();
        }
    }

    /// <summary>
    /// Details of the version associated with a <see cref="AutomationProject"/>
    /// </summary>
    [DataContract]
    [Serializable]
    public class ProjectVersion : VersionInfo
    {
        /// <summary>
        /// constructor
        /// </summary>
        public ProjectVersion() : this(new Version(1, 0))
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public ProjectVersion(Version version)
        {
            this.Version = version;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public ProjectVersion(string version) : this(new Version(version))
        {

        }
    }

    /// <summary>
    /// Details of the version associated with a <see cref="PrefabProject"/>
    /// </summary>
    [DataContract]
    [Serializable]
    public class PrefabVersion : VersionInfo
    {
        /// <summary>
        /// constructor
        /// </summary>
        public PrefabVersion() : this(new Version(1, 0))
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public PrefabVersion(Version version)
        {
            this.Version = version;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public PrefabVersion(string version) : this(new Version(version))
        {

        }
    }
}
