using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    /// <summary>
    /// Captures version details such as version, deployed, etc.
    /// </summary>
    [DataContract]
    [Serializable]
    public class VersionInfo
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
        /// DateTime when the version was publisheds
        /// </summary>
        [DataMember(IsRequired = false, Order = 40)]
        public DateTime? PublishedOn { get; set; }

        /// <summary>
        /// Description 
        /// </summary>
        [DataMember(IsRequired = false, Order = 40)]
        public string Description { get; set; }

        public bool IsPublished => !this.IsActive;

        /// <summary>
        /// constructor
        /// </summary>
        public VersionInfo() : this(new Version(1, 0))
        {
        }

        /// <summary>
        /// constructor
        /// </summary>
        public VersionInfo(Version version)
        {
            this.Version = version;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public VersionInfo(string version) : this(new Version(version))
        {

        }

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
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Version, this.IsActive);
        }

        ///</inheritdoc>
        public override string ToString()
        {
            return this.Version.ToString();
        }
    }    
}
