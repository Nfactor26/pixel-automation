using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Persistence.Core.Models
{
    [BsonIgnoreExtraElements]
    public class TestResult
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string TestId { get; set; }

        public string TestName { get; set; }

        public string CategoryId { get; set; }

        public string CategoryName { get; set; }

        public TestState Result { get; set; }

        /// <summary>
        /// Time taken by test to execute in seconds
        /// </summary>
        public double ExecutionTime { get; set; }

        public string ErrorMessage { get; set; }

    }
}
