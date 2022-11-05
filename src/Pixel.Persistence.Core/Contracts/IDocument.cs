using System;

namespace Pixel.Persistence.Core.Contracts;

public interface IDocument<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Identifier for the document
    /// </summary>
    TKey Id { get; set; }

    /// <summary>
    /// Version of the document
    /// </summary>
    int Revision { get; set; }

    /// <summary>
    /// Indicates if a document has been soft deleted
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Date and time when document was created
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when document was last updated
    /// </summary>
    DateTime LastUpdated { get; set; }
}
