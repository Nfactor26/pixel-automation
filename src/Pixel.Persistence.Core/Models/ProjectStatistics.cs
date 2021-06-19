using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class ProjectStatistics
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [DataMember]
        public string ProjectId { get; set; }

        [DataMember]
        public string ProjectName { get; set; }

        /// <summary>
        /// Historical execution statistics on a monthly basis
        /// </summary>
        [DataMember]
        public List<ProjectExecutionStatistics> MonthlyStatistics { get; set; } = new List<ProjectExecutionStatistics>();     
      
    }
}
