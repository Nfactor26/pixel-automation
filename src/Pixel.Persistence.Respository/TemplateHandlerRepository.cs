using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository;

/// <summary>
/// Default implementation of <see cref="ITemplateHandlerRepository"/>
/// </summary>
public class TemplateHandlerRepository : ITemplateHandlerRepository
{
    private readonly ILogger logger;
    private readonly IMongoCollection<TemplateHandler> handlersCollection;

    private static readonly InsertOneOptions InsertOneOptions = new InsertOneOptions();
    private static readonly FindOptions<TemplateHandler> FindOptions = new FindOptions<TemplateHandler>();
    private static readonly FindOneAndUpdateOptions<TemplateHandler> FindOneAndUpdateOptions = new FindOneAndUpdateOptions<TemplateHandler>();

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="dbSettings"></param>
    public TemplateHandlerRepository(ILogger<TemplateHandlerRepository> logger, IMongoDbSettings dbSettings)
    {
        this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
        var client = new MongoClient(dbSettings.ConnectionString);
        var database = client.GetDatabase(dbSettings.DatabaseName);
        handlersCollection = database.GetCollection<TemplateHandler>(dbSettings.TemplateHandlersName);
    }

    /// <inheritdoc/>  
    public async Task<TemplateHandler> FindByIdAsync(string Id, CancellationToken cancellationToken)
    {
        Guard.Argument(Id, nameof(Id)).NotNull().NotEmpty();
        var filter = Builders<TemplateHandler>.Filter.And(Builders<TemplateHandler>.Filter.Eq(x => x.Id, Id));
        return (await handlersCollection.FindAsync(filter, FindOptions, cancellationToken)).FirstOrDefault();
    }

    /// <inheritdoc/>  
    public async Task<TemplateHandler> FindByNameAsync(string name, CancellationToken cancellationToken)
    {
        Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
        var filter = Builders<TemplateHandler>.Filter.And(Builders<TemplateHandler>.Filter.Eq(x => x.Name, name));
        return (await handlersCollection.FindAsync(filter, FindOptions, cancellationToken)).FirstOrDefault();
    }

    /// <inheritdoc/>  
    public async Task<IEnumerable<TemplateHandler>> GetAllAsync(CancellationToken cancellationToken)
    {
        var filter = Builders<TemplateHandler>.Filter.Empty;
        var handlers = await handlersCollection.FindAsync(filter, FindOptions, cancellationToken);
        return await handlers.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TemplateHandler>> GetHandlersAsync(GetHandlersRequest queryParameter)
    {
        Guard.Argument(queryParameter).NotNull();

        var filterBuilder = Builders<TemplateHandler>.Filter;
        var filter = filterBuilder.Empty;
        if (!string.IsNullOrEmpty(queryParameter.HandlerFilter))
        {
            filter = filterBuilder.And(filter, filterBuilder.Regex(t => t.Name, new MongoDB.Bson.BsonRegularExpression(queryParameter.HandlerFilter)));            
        }
        var sort = Builders<TemplateHandler>.Sort.Descending(nameof(TemplateHandler.Name));
        var all = handlersCollection.Find(filter).Sort(sort).Skip(queryParameter.Skip).Limit(queryParameter.Take);
        var result = await all.ToListAsync();
        return result ?? Enumerable.Empty<TemplateHandler>();
    }

    /// <inheritdoc/>  
    public async Task AddHandlerAsync(TemplateHandler handler, CancellationToken cancellationToken)
    {
        Guard.Argument(handler).NotNull();
        var exists = (await FindByNameAsync(handler.Name, cancellationToken)) != null;
        if (exists)
        {
            throw new InvalidOperationException($"Handler with name {handler.Name} already exists.");
        }
        await handlersCollection.InsertOneAsync(handler, InsertOneOptions, cancellationToken).ConfigureAwait(false);
        logger.LogInformation("Handler {0} was added.", handler.Name);
    }

    /// <inheritdoc/>  
    public async Task UpdateHandlerAsync(TemplateHandler handler, CancellationToken cancellationToken)
    {
        Guard.Argument(handler).NotNull();
        var filter = Builders<TemplateHandler>.Filter.Eq(f => f.Id, handler.Id);
        var updateDefinition = Builders<TemplateHandler>.Update
          .Set(t => t.Parameters, handler.Parameters)
          .Set(t => t.Description, handler.Description)            
          .Set(t => t.LastUpdated, DateTime.UtcNow)
          .Inc(t => t.Revision, 1);
        if(handler is DockerTemplateHandler dockerTemplateHandler)
        {
            updateDefinition = updateDefinition.Set(t => (t as DockerTemplateHandler).DockerComposeFileName, dockerTemplateHandler.DockerComposeFileName);
        }

        await handlersCollection.FindOneAndUpdateAsync(filter, updateDefinition, FindOneAndUpdateOptions, cancellationToken);       
        logger.LogInformation("Handler {0} was updated.", handler.Name);
    }

    /// <inheritdoc/>  
    public async Task DeleteHandlerAsync(string Id, CancellationToken cancellationToken)
    {
        var filter = Builders<TemplateHandler>.Filter.Eq(f => f.Id, Id);
        var updateDefinition = Builders<TemplateHandler>.Update
            .Set(t => t.IsDeleted, true)
            .Set(t => t.LastUpdated, DateTime.UtcNow)
            .Inc(t => t.Revision, 1);
        await handlersCollection.FindOneAndUpdateAsync(filter, updateDefinition, FindOneAndUpdateOptions, cancellationToken);
        logger.LogInformation("Handler with Id : {0} was deleted", Id);
    }
}
