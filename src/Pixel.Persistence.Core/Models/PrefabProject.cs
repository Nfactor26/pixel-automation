using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models;

[DataContract]
public class PrefabProject : Document
{
    [DataMember(IsRequired = true, Order = 10)]   
    public string ApplicationId { get; set; }

    [DataMember(IsRequired = true, Order = 20)] 
    public string ProjectId { get; set; }

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
    public List<VersionInfo> AvailableVersions { get; set; } = new ();

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
    
}
