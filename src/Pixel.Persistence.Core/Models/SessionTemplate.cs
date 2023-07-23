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
        /// InitializeEnvironment.csx can have multiple initializer functions. By default InitializeDefault() will be executed.
        /// However, templates can provide a custom function name instead.
        /// </summary>
        [DataMember(IsRequired = false)]
        public string InitFunction { get; set; } = "InitializeDefault()";

        /// <summary>
        /// Trigger associated with the template 
        /// </summary>
        [DataMember(IsRequired = false)]
        public List<SessionTrigger> Triggers { get; set; } = new();
     
    }
}
