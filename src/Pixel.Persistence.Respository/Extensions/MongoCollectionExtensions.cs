using MongoDB.Driver;
using Pixel.Persistence.Core.Contracts;
using Pixel.Persistence.Respository.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository.Extensions;

public static class MongoCollectionExtensions
{
    public static async Task<bool> UpdateField<TDocument, TKey, TField>(this IMongoCollection<TDocument> collection,
        TDocument document,
        Expression<Func<TDocument, TField>> field, TField value)
        where TKey : IEquatable<TKey>
        where TDocument : IDocument<TKey>
    {
        var filterBuilder = Builders<TDocument>.Filter;
        var filter = filterBuilder.Eq(t => t.Id, document.Id);
        var updateBuilder = Builders<TDocument>.Update;
        var update = updateBuilder.Set(field, value);
        var result = await collection.UpdateOneAsync(filter, update);
        return result.IsAcknowledged && result.ModifiedCount == 1;
    }

    public static async Task<TItem> FindFirstOrDefaultAsync<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, bool>> p, CancellationToken cancellationToken = default)
    {
        return await (await collection.FindAsync(p, FindOptionsFactory.LimitTo<TItem>(1), cancellationToken).ConfigureAwait(false)).FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public static async Task<IEnumerable<TItem>> WhereAsync<TItem>(this IMongoCollection<TItem> collection, Expression<Func<TItem, bool>> p, CancellationToken cancellationToken = default)
    {
        return (await collection.FindAsync(p, cancellationToken: cancellationToken).ConfigureAwait(false)).ToEnumerable();
    }
}
