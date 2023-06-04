using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class SessionTemplate : Document
    {       
        /// <summary>
        /// Name of the template
        /// </summary>
        [Required]
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        ///  Id of the Project executed in test session
        /// </summary>
        [Required]
        [DataMember]
        public string ProjectId { get; set; }

        /// <summary>
        /// Name of the Project executed in test session
        /// </summary>
        [Required]
        [DataMember]
        public string ProjectName { get; set; }

        /// <summary>
        /// Version of the Project to use
        /// </summary>
        [Required]
        [DataMember]
        public string TargetVersion { get; set; }
        
        /// <summary>
        /// Query script for selecting the tests
        /// </summary>
        [Required]
        [DataMember]
        public string Selector { get; set; }
      
        /// <summary>
        /// Script file (*.csx) override that can be used to initialize the process data model e.g. 
        /// By default InitializeEnvironment.csx generated at design time is used
        /// </summary>
        [DataMember(IsRequired = false)]
        public string InitializeScript { get; set; }

        /// <summary>
        /// Trigger associated with the template 
        /// </summary>
        [DataMember(IsRequired = false)]
        public List<SessionTrigger> Triggers { get; set; } = new();
     
    }
}
