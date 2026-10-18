using Pixel.Automation.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models;

[DataContract]
[Serializable]
[FileDescription("prefab")]
public class Prefab : ICloneable
{
    /// <summary>
    /// Identifier of the Prefab
    /// </summary>
    [DataMember(IsRequired = true, Order = 10)]
    public string PrefabId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifier of the application to which Prefab belongs
    /// </summary>
    [DataMember(IsRequired = true, Order = 20)]
    public string ApplicationId { get; set;  }
  
    /// <summary>
    /// Display name for the Prefab
    /// </summary>
    [DataMember(IsRequired = true, Order = 30)]
    public string DisplayName { get; set; }  
      
    /// <summary>
    /// Description for the Prefab
    /// </summary>
    [DataMember(IsRequired = false, Order = 40)]
    public string Description { get; set; }

    /// <summary>
    /// Indicates if the Prefab is deleted. Deleted prefabs are not loaded in explorer.
    /// </summary>
    [DataMember(IsRequired = false, Order = 1000)]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Get all the versions created for this Prefab
    /// </summary>
    [DataMember(IsRequired = true, Order = 50)]
    public List<VersionInfo> AvailableVersions { get; set; } = new List<VersionInfo>();

    /// <inheritdoc/>  
    public IEnumerable<VersionInfo> ActiveVersions { get => AvailableVersions.Where(a => a.IsActive).ToList(); }

    /// <inheritdoc/>  
    public IEnumerable<VersionInfo> PublishedVersions { get => AvailableVersions.Where(a => a.IsPublished).ToList(); }

    /// <inheritdoc/>  
    public VersionInfo LatestActiveVersion
    {
        get => this.ActiveVersions.OrderBy(a => a.Version).LastOrDefault();
    }

    ///</inheritdoc>
    public object Clone()
    {
        Prefab copy = new Prefab()
        {
            DisplayName = this.DisplayName,
            Description = this.Description,          
            ApplicationId = this.ApplicationId
        };
        return copy;
    }

    ///</inheritdoc>
    public override string ToString()
    {
        return $"{PrefabId}";
    }

}
