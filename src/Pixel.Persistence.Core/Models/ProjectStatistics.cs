using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Linq;
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

      
        [BsonIgnore]
        public int NumberOfTestsExeucted
        {
            get => MonthlyStatistics.Select(m => m.NumberOfTestsExecuted).Sum();
        }

        [BsonIgnore]
        public int NumberOfTestsFailed
        {
            get => MonthlyStatistics.Select(m => m.NumberOfTestsFailed).Sum();
        }

        [BsonIgnore]
        public int NumberOfTestsPassed
        {
            get => MonthlyStatistics.Select(m => m.NumberOfTestsPassed).Sum();
        }

        [BsonIgnore]
        public double SuccessRate
        {
            get => (NumberOfTestsPassed / NumberOfTestsExeucted) * 100;
        }
    }
}
