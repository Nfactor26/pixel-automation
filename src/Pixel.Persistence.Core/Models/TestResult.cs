using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pixel.Persistence.Core.Enums;
using System;

namespace Pixel.Persistence.Core.Models
{
    [BsonIgnoreExtraElements]
    public class TestResult
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string SessionId { get; set; }
       
        public string ProjectId { get; set; }
      
        public string ProjectName { get; set; }

        public string TestId { get; set; }
       
        public string TestName { get; set; }

        public string FixtureId { get; set; }

        public string FixtureName { get; set; }

        public TestStatus Result { get; set; }

        /// <summary>
        /// Indicates the order in which test was executed
        /// </summary>
        public int ExecutionOrder { get; set; }

        /// <summary>
        /// Date on which test was executed
        /// </summary>
        public DateTime ExecutedOn { get; set; } 

        /// <summary>
        /// Time taken by test to execute in seconds
        /// </summary>
        public double ExecutionTime { get; set; }

        public FailureDetails FailureDetails { get; set; }

        public bool IsProcessed { get; set; }

    }
}
