using System;
using System.ComponentModel.DataAnnotations;

namespace Pixel.Persistence.Core.Models
{
    public class ControlMetaData
    {
        [Required]
        public string ApplicationId { get; set; }

        [Required]
        public string ControlId { get; set; }

        [Required]
        public string ControlName { get; set; }

        public DateTime LastUpdated { get; set; }
    }

    public class ControlImageMetaData
    {
        [Required]
        public string ApplicationId { get; set; }

        [Required]
        public string ControlId { get; set; }    

        [Required]
        public string Resolution { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
