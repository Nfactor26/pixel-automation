using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    public class ProjectReferences
    {
        [DataMember(IsRequired = true, Order = 20)]
        public EditorReferences EditorReferences { get; set; } = new EditorReferences();

        [DataMember(IsRequired = true, Order = 20)]
        public List<ControlReference> ControlReferences { get; set; } = new();

        [DataMember(IsRequired = true, Order = 30)]
        public List<PrefabReference> PrefabReferences { get; set; } = new();
    }
}
