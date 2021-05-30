using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{ 
    [DataContract]
    [Serializable]
    public class TestStatistics
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [DataMember]
        public string ProjectId { get; set; }

        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public string TestId { get; set; }

        [DataMember]
        public string TestName { get; set; }

        [DataMember]
        public string FixtureId { get; set; }

        [DataMember]
        public string FixtureName { get; set; }

        [DataMember]
        List<Label> Annotations { get; set; } = new List<Label>();

        /// <summary>
        /// Historical execution statistics on a monthly basis
        /// </summary>
        [DataMember]
        public List<TestExecutionStatistics> MonthlyStatistics { get; set; } = new List<TestExecutionStatistics>();

        /// <summary>
        /// Collection of failure due to unique reasons over the period of this execution statistics
        /// </summary>
        [DataMember]
        public List<FailureDetails> UniqueFailures { get; set; } = new List<FailureDetails>();
        
    }
        
}
