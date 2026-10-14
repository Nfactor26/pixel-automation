using System.Collections.Generic;
using System.Runtime.Serialization;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Core.Models;

/// <summary>
/// Resources belonging to or used by <see cref="IProject"/>
/// </summary>
[DataContract]
public class ProjectReferences
{
    [DataMember(IsRequired = true, Order = 10)]
    public EditorReferences EditorReferences { get; set; } = new EditorReferences();

    [DataMember(IsRequired = true, Order = 20)]
    public List<ControlReference> ControlReferences { get; set; } = new();

    [DataMember(IsRequired = true, Order = 30)]
    public List<PrefabReference> PrefabReferences { get; set; } = new();

    [DataMember(IsRequired = true, Order = 40)]
    public List<string> Fixtures { get; set; } = new();

    [DataMember(IsRequired = true, Order = 50)]
    public GroupedCollection<string> TestDataSources { get; set; } = new();
 
}
