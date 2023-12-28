using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class ProjectReferences : Document
    {
        [DataMember(IsRequired = true, Order = 10)]
        public string ProjectId { get; set; }

        [DataMember(IsRequired = true, Order = 20)]
        public string ProjectVersion { get; set; }

        [DataMember(IsRequired = true, Order = 30)]
        public EditorReferences EditorReferences { get; set; } = new EditorReferences();

        [DataMember(IsRequired = true, Order = 40)]
        public List<ControlReference> ControlReferences { get; set; } = new();

        [DataMember(IsRequired = true, Order = 50)]
        public List<PrefabReference> PrefabReferences { get; set; } = new();

        [DataMember(IsRequired = true, Order = 60)]
        public List<string> Fixtures { get; set; } = new();
              
        [DataMember(IsRequired = true, Order = 70)]
        public GroupedCollection<string> TestDataSources { get; set; } = new();

    }
}
