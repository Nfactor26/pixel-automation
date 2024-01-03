using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pixel.Persistence.Core.Enums;
using System;
using System.Runtime.InteropServices;

namespace Pixel.Persistence.Core.Models
{
    /// <summary>
    /// TestSession represents an execution of a group of test cases executed in an adhoc manner or using 
    /// a predefined test template.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class TestSession : Document
    {
        /// <summary>
        /// Id of the Session Template that was used to start this session
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string TemplateId { get; set; }

        /// <summary>
        /// Name of the Session Template that was used to start this session
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// Id of the Project executed in test session
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Name of the Project executed in test session
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Version of the project executed in test session
        /// </summary>
        public string ProjectVersion { get; set; }

        /// <summary>
        /// Name of the Machine where test session was executed
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Operating System on which test session was execued
        /// </summary>
        public string OSDetails { get; set; }
      

        /// <summary>
        /// Date and time when the test session was executed
        /// </summary>
        public DateTime SessionStartTime { get; set;}

        /// <summary>
        /// Total session time in minutes
        /// </summary>
        public double SessionTime { get; set; }

        /// <summary>
        /// Number of tests executed in the session
        /// </summary>     
        public int TotalNumberOfTests { get; set; }


        /// <summary>
        /// Number of tests passed in the session
        /// </summary>      
        public int NumberOfTestsPassed { get; set; }


        /// <summary>
        /// Number of tests failed in the session
        /// </summary>      
        public int NumberOfTestsFailed { get; set; }

        /// <summary>
        /// Indicates whether Session is in progress or completed
        /// </summary>      
        public SessionStatus SessionStatus { get; set; }

        /// <summary>
        /// Indicates whether all the tests passed in the session
        /// </summary>      
        public TestStatus SessionResult { get; set; }

        public bool IsProcessed { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public TestSession()
        {
            this.SessionStartTime = DateTime.Now.ToUniversalTime();
            this.SessionStatus = SessionStatus.InProgress;
            this.MachineName = Environment.MachineName;
            this.OSDetails = RuntimeInformation.OSDescription;
        }
    }
}
