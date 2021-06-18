using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class SessionTemplate
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

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
        /// Version of the project executed in test session
        /// </summary>
        [Required]
        [DataMember]
        public string ProjectVersion { get; set; }

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
     
    }
}
