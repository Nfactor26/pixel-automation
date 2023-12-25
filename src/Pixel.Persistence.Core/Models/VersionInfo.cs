using System;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models;

/// <summary>
/// Captures version details such as version, deployed, etc.
/// </summary>
[DataContract]
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


    [DataMember(IsRequired = false, Order = 40)]
    public DateTime? PublishedOn { get; set; }


    public override bool Equals(object obj)
    {
        if(obj is VersionInfo versionInfo)
        {
            return versionInfo.Version.Equals(this.Version);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Version, IsActive);
    }

    public override string ToString()
    {
        return Version.ToString();
    }

}
