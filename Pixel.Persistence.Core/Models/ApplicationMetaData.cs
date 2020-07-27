using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Pixel.Persistence.Core.Models
{
    public class ApplicationMetaData
    {
        [Required]
        public string ApplicationId { get; set; }
               
        public string ApplicationName { get; set; }
    
        public string ApplicationType { get; set; }
    
        public DateTime LastUpdated { get; set; }
    }
}
