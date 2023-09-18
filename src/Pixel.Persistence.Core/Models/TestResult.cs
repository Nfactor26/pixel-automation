using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pixel.Persistence.Core.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [BsonIgnoreExtraElements]
    [DataContract]
    public class TestResult : Document
    {
        /// <summary>
        /// Identifier of the session in which the test was executed
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        [DataMember(IsRequired = true, Order = 20)]
        public string SessionId { get; set; }

        /// <summary>
        /// Identifier of the project to which test case belongs
        /// </summary>
        [DataMember(IsRequired = true, Order = 30)]
        public string ProjectId { get; set; }

        /// <summary>
        /// Name of the project to which test case belongs
        /// </summary>
        [DataMember(IsRequired = true, Order = 40)]
        public string ProjectName { get; set; }

        /// <summary>
        /// Version of the project for which test case was executed
        /// </summary>
        [DataMember(IsRequired = true, Order = 50)]
        public string ProjectVersion { get; set; }

        /// <summary>
        /// Identifier of the fixture to which test belongs
        /// </summary>
        [DataMember(IsRequired = true, Order = 60)]
        public string FixtureId { get; set; }

        /// <summary>
        /// Name of the fixture to which test belongs
        /// </summary>
        [DataMember(IsRequired = true, Order = 70)]
        public string FixtureName { get; set; }

        /// <summary>
        /// Identifier of the test case that was executed
        /// </summary>
        [DataMember(IsRequired = true, Order = 80)]
        public string TestId { get; set; }

        /// <summary>
        /// Name of the test case that was executed
        /// </summary>
        [DataMember(IsRequired = true, Order = 90)]
        public string TestName { get; set; }

        /// <summary>
        /// Test case data with which test was executed
        /// </summary>
        [DataMember(IsRequired = true, Order = 95)]
        public string TestData { get; set; }

        /// <summary>
        /// Result of execution
        /// </summary>
        [DataMember(IsRequired = true, Order = 100)]
        public TestStatus Result { get; set; }

        /// <summary>
        /// Indicates the order in which test was executed
        /// </summary>
        [DataMember(IsRequired = true, Order = 110)]
        public int ExecutionOrder { get; set; }

        /// <summary>
        /// Date on which test was executed
        /// </summary>
        [DataMember(IsRequired = true, Order = 120)]
        public DateTime ExecutedOn { get; set; }

        /// <summary>
        /// Time taken by test to execute in seconds
        /// </summary>
        [DataMember(IsRequired = true, Order = 130)]
        public double ExecutionTime { get; set; }

        /// <summary>
        /// Trace message and screenshots capture while executing test case
        /// </summary>
        [DataMember(IsRequired = true, Order = 140)]
        public List<TraceData> Traces { get; set; } = new();

        /// <summary>
        /// Details of failure in case the test execution failed
        /// </summary>
        [DataMember(IsRequired = false, Order = 150)]
        public FailureDetails FailureDetails { get; set; }

        /// <summary>
        /// Indicates if the test case data has been processed to update various statistics
        /// </summary>
        [DataMember(IsRequired = false, Order = 160)]
        public bool IsProcessed { get; set; }

    }


    /// <summary>
    /// Represents a trace data for various steps of automation
    /// </summary>
    [DataContract]
    [JsonDerivedType(typeof(MessageTraceData), typeDiscriminator: nameof(MessageTraceData))]
    [JsonDerivedType(typeof(ImageTraceData), typeDiscriminator: nameof(ImageTraceData))] 
    [BsonKnownTypes(typeof(MessageTraceData))]
    [BsonKnownTypes(typeof(ImageTraceData))]  
    public abstract class TraceData
    {
        [DataMember(IsRequired = true, Order = 10)]
        public DateTime RecordedAt { get; set; }

        [DataMember(IsRequired = true, Order = 20)]
        public TraceLevel TraceLevel { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public TraceData()
        {

        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="recordedAt"></param>
        /// <param name="traceLevel"></param>
        public TraceData(DateTime recordedAt, TraceLevel traceLevel)
        {
            this.RecordedAt = recordedAt;
            this.TraceLevel = traceLevel;
        }
    }

    /// <summary>
    /// Represents a log entry for a trace data for various steps of automation
    /// </summary>
    [DataContract]
    [JsonDerivedType(typeof(MessageTraceData), typeDiscriminator: nameof(MessageTraceData))]
    public class MessageTraceData : TraceData
    {
        /// <summary>
        /// Trace message
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        public string Messsage { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public MessageTraceData()
        {

        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="recordedAt"></param>
        /// <param name="traceLevel"></param>
        /// <param name="messsage"></param>
        public MessageTraceData(DateTime recordedAt, TraceLevel traceLevel, string messsage) : base(recordedAt, traceLevel)
        {
            this.Messsage = messsage;
        }
    }

    /// <summary>
    /// Represents a image entry for a trace data for various steps of automation
    /// </summary>
    [DataContract]
    [JsonDerivedType(typeof(ImageTraceData), typeDiscriminator: nameof(ImageTraceData))]
    public class ImageTraceData : TraceData
    {
        /// <summary>
        /// Trace image file
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        public string ImageFile { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        public ImageTraceData()
        {

        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="recordedAt"></param>
        /// <param name="traceLevel"></param>
        /// <param name="imageFile"></param>
        public ImageTraceData(DateTime recordedAt, TraceLevel traceLevel, string imageFile) : base(recordedAt, traceLevel)
        {
            this.ImageFile = imageFile;
        }
    }
}
