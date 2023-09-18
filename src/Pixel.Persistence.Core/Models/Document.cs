using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pixel.Persistence.Core.Contracts;
using System;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models;

/// <summary>
/// Identifies a document to be stored in a document database
/// </summary>
public abstract class Document : IDocument<string>
{
    /// <summary>
    /// Identifier of the document
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [DataMember(IsRequired = true, Order = 1000)]
    public string Id { get; set; }

    /// <summary>
    /// Revision of the document
    /// </summary>
    [DataMember(IsRequired = true, Order = 1010)]
    public int Revision { get; set; } = 1;

    /// <summary>
    /// Indicates if document is marked deleted
    /// </summary>
    [DataMember(IsRequired = false, Order = 1020)]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Indicates the date when document was created
    /// </summary>
    [DataMember(IsRequired = true, Order = 1030)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates the date when document was last updated
    /// </summary>
    [DataMember(IsRequired = true, Order = 1040)]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
