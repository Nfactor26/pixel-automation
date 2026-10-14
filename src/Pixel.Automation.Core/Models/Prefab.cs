using Pixel.Automation.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

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
    /// Version of the control
    /// </summary>
    [DataMember(Order = 40)]
    public Version Version { get; set; }

    /// <summary>
    /// Group name to which the control belongs. "Default" is the default group.
    /// </summary>
    [DataMember(Order = 50)]
    public string GroupName { get; set; } = "Default";

    /// <summary>
    /// Description for the Prefab
    /// </summary>
    [DataMember(IsRequired = false, Order = 40)]
    public string Description { get; set; }
   
    /// <summary>
    /// Script file for the test case. This script file can be used for any temporary variables required 
    /// during test execution.
    /// </summary>
    [DataMember(IsRequired = true, Order = 100)]
    public string ScriptFile { get; set; }

    /// <summary>
    /// Collection of Identifiers of controls used by the Prefab
    /// </summary>
    [DataMember(IsRequired = true, Order = 130)]
    public List<ControlUsage> ControlsUsed { get; private set; } = new();  

    /// <summary>
    /// Indicates if the Prefab is deleted. Deleted prefabs are not loaded in explorer.
    /// </summary>
    [DataMember(IsRequired = false, Order = 1000)]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Root entity for the Prefab
    /// </summary>
    [JsonIgnore]
    public Entity WorkFlowEntity { get; set; }

    ///</inheritdoc>
    public object Clone()
    {
        Prefab copy = new Prefab()
        {
            DisplayName = this.DisplayName,
            Description = this.Description,          
            ApplicationId = this.ApplicationId,
            WorkFlowEntity = this.WorkFlowEntity
        };
        return copy;
    }

    ///</inheritdoc>
    public override string ToString()
    {
        return $"{PrefabId}";
    }

}
