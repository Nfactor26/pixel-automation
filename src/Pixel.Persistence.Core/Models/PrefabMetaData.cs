using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class PrefabMetaData
    {
        [Required]
        [DataMember]
        public string PrefabId { get; set; }

        [Required]
        [DataMember]
        public string ApplicationId { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

    }

    [DataContract]
    public class PrefabMetaDataCompact
    {
        [Required]
        [DataMember]
        public string PrefabId { get; set; }

        [Required]
        [DataMember]
        public string ApplicationId { get; set; }

        [DataMember]
        public DateTime LastUpdated { get; set; }

        [DataMember]
        public List<PrefabVersionMetaData> Versions { get; set; } = new List<PrefabVersionMetaData>();

        public void AddOrUpdateVersionMetaData(PrefabVersionMetaData prefabVersionMetaData)
        {
            var existingEntry = Versions.FirstOrDefault(v => v.Version.Equals(prefabVersionMetaData.Version));
            if (existingEntry != null)
            {
                Versions.Remove(existingEntry);              
            }
            Versions.Add(prefabVersionMetaData);         
        }
    }

    [DataContract]
    public class PrefabVersionMetaData
    {
        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public DateTime LastUpdated { get; set; }
    }
}
