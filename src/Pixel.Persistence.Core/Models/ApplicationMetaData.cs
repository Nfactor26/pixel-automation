using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class ApplicationMetaData
    {
        [Required]
        [DataMember]
        public string ApplicationId { get; set; }
               
        [DataMember]
        public string ApplicationName { get; set; }
    
        [DataMember]
        public string ApplicationType { get; set; }
    
        [DataMember]
        public DateTime LastUpdated { get; set; }

        [DataMember]
        public List<ControlMetaData> ControlsMeta { get; set; } = new List<ControlMetaData>();

        [DataMember]
        public List<PrefabMetaDataCompact> PrefabsMeta { get; set; } = new List<PrefabMetaDataCompact>();

        public void AddOrUpdatePrefabMetaData(string prefabId, string version, bool isActive)
        {
            var prefabMetaDataEntry = PrefabsMeta.FirstOrDefault(p => p.PrefabId.Equals(prefabId));
            if(prefabMetaDataEntry != null)
            {
                prefabMetaDataEntry.AddOrUpdateVersionMetaData(new PrefabVersionMetaData() { Version = version, IsActive = isActive, LastUpdated = DateTime.UtcNow });
                return;
            }
            var prefabMetaData = new PrefabMetaDataCompact() { PrefabId = prefabId };
            prefabMetaData.AddOrUpdateVersionMetaData(new PrefabVersionMetaData() { Version = version, IsActive = isActive, LastUpdated = DateTime.UtcNow });
            this.PrefabsMeta.Add(prefabMetaData);
        }
               
    }
}
