using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pixel.Persistence.Core.Contracts;
using System;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models;

public abstract class Document : IDocument<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [DataMember(IsRequired = true, Order = 1000)]
    public string Id { get; set; }

    [DataMember(IsRequired = true, Order = 1010)]
    public int Revision { get; set; } = 1;

    [DataMember(IsRequired = false, Order = 1020)]
    public bool IsDeleted { get; set; }

    [DataMember(IsRequired = true, Order = 1030)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [DataMember(IsRequired = true, Order = 1040)]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
