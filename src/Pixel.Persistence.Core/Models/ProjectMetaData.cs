using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pixel.Persistence.Core.Models
{
    public class ProjectMetaData
    {
        [Required]
        public string ProjectId { get; set; }
       
        public string Version { get; set; }

        public string Type { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeployed { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
