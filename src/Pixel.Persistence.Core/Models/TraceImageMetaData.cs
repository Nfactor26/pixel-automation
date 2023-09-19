using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models;

/// <summary>
/// MetaData for a trace image captured during execution of test case
/// </summary>
[DataContract]
public class TraceImageMetaData
{
    /// <summary>
    /// Identifier of the session which executed the test case
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    [DataMember(IsRequired = true, Order = 10)]
    public string SessionId { get; set; }

    /// <summary>
    /// Identifier of the TestResult to which trace image should be associated
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    [DataMember(IsRequired = true, Order = 20)]
    public string TestResultId { get; set; }  
}
