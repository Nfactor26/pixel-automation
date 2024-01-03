using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{ 

    [DataContract]
    public class ControlImageMetaData
    {
        [Required]
        [DataMember]
        public string ApplicationId { get; set; }

        [Required]
        [DataMember]
        public string ControlId { get; set; }

        [Required]
        [DataMember]
        public string Version { get; set; }

        [Required]
        [DataMember]
        public string FileName { get; set; }        

        [DataMember]
        public DateTime LastUpdated { get; set; }
    }
}
