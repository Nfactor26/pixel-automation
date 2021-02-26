using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Persistence.Core.Models
{
    [BsonIgnoreExtraElements]
    public class TestSession
    {      
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SessionId { get; set; }

        public string MachineName { get; set; }

        public string OSDetails { get; set; }     
        
        public string ProjectName { get; set; }

        public string ProjectVersion { get; set; }

        public TestSession()
        {
           
        }

        public TestSession(string projectName, string projectVersion)
        {
            this.ProjectName = projectName;
            this.ProjectVersion = projectVersion;
        }
      

       /// <summary>
       /// Total session time in minutes
       /// </summary>
        public double SessionTime
        {
            get
            {
                double sessionTime = 0;
                foreach (var result in TestResultCollection)
                {
                    sessionTime += result.ExecutionTime;
                }
                return sessionTime / 60;
            }

        }

        public List<TestResult> TestResultCollection { get; set; } = new List<TestResult>();

        [BsonIgnore]
        public int TotalNumberOfTests
        {
            get => this.TestResultCollection.Count();
        }
     
        [BsonIgnore]
        public int NumberOfTestsPassed
        {
            get => this.TestResultCollection.Where(a => a.Result.Equals(TestState.Success)).Count();
        }
      
        [BsonIgnore]
        public int NumberOfTestsFailed
        {
            get => this.TestResultCollection.Where(a => !a.Result.Equals(TestState.Success)).Count();
        }

        [BsonIgnore]
        public string SessionResult
        {
            get
            {
                if (TestResultCollection.All(a => a.Result.Equals(TestState.Success)))
                {
                    return "Passed";
                }
                return "Fail";
            }
        }
      
    }
}
