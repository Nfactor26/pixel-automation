using System;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models;

/// <summary>
/// Maps Prefabs to TestCases that have a reference to this Prefab
/// </summary>
[DataContract]
public class PrefabReference
{
    /// <summary>
    /// ApplicationId to which Prefab belongs
    /// </summary>
    [DataMember(IsRequired = true, Order = 10)]
    public string ApplicationId { get; set; }

    /// <summary>
    /// Identifier of Prefab
    /// </summary>
    [DataMember(IsRequired = true, Order = 20)]
    public string PrefabId { get; set; }

    /// <summary>
    /// Version of Prefab in use
    /// </summary>
    [DataMember(IsRequired = false, EmitDefaultValue = false, Order = 30)]
    public VersionInfo Version { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public PrefabReference()
    {

    }

    public override bool Equals(object obj)
    {
        if (obj is PrefabReference prefabReference)
        {
            return prefabReference.ApplicationId.Equals(this.ApplicationId) && prefabReference.PrefabId.Equals(this.PrefabId);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ApplicationId, PrefabId);
    }
}
