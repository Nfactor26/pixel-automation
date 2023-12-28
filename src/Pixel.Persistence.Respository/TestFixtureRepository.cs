using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository;

public class TestFixtureRepository : ITestFixtureRepository
{
    private readonly ILogger logger;
    private readonly IMongoCollection<TestFixture> fixturesCollection;
    private readonly IMongoCollection<TestCase> testsCollection;
    private readonly IReferencesRepository referencesRepository;

    private static readonly InsertOneOptions InsertOneOptions = new InsertOneOptions();
    private static readonly FindOptions<TestFixture> FindOptions = new FindOptions<TestFixture>();
    private static readonly FindOneAndUpdateOptions<TestFixture> FindOneAndUpdateOptions = new FindOneAndUpdateOptions<TestFixture>();
   
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="dbSettings"></param>
    public TestFixtureRepository(ILogger<TestFixtureRepository> logger, IMongoDbSettings dbSettings, IReferencesRepository referencesRepository)
    {
        this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
        var client = new MongoClient(dbSettings.ConnectionString);
        var database = client.GetDatabase(dbSettings.DatabaseName);
        fixturesCollection = database.GetCollection<TestFixture>(dbSettings.FixturesCollectionName);
        testsCollection = database.GetCollection<TestCase>(dbSettings.TestsCollectionName);
        this.referencesRepository = Guard.Argument(referencesRepository, nameof(referencesRepository)).NotNull().Value;
    }

    /// <inheritdoc/>  
    public async Task<TestFixture> FindByIdAsync(string projectId, string projectVersion, string fixtureId, CancellationToken cancellationToken)
    {
        var filter = Builders<TestFixture>.Filter.And(Builders<TestFixture>.Filter.Eq(x => x.ProjectId, projectId),
                            Builders<TestFixture>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                            Builders<TestFixture>.Filter.Eq(x => x.FixtureId, fixtureId));
        return (await fixturesCollection.FindAsync(filter, FindOptions, cancellationToken)).FirstOrDefault();
    }

    /// <inheritdoc/>  
    public async Task<TestFixture> FindByNameAsync(string projectId, string projectVersion, string name, CancellationToken cancellationToken)
    {
        var filter = Builders<TestFixture>.Filter.And(Builders<TestFixture>.Filter.Eq(x => x.ProjectId, projectId),
                            Builders<TestFixture>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                            Builders<TestFixture>.Filter.Eq(x => x.DisplayName, name));
        return (await fixturesCollection.FindAsync(filter, FindOptions, cancellationToken)).FirstOrDefault();
    }

    /// <inheritdoc/>  
    public async Task<IEnumerable<TestFixture>> GetFixturesAsync(string projectId, string projectVersion, DateTime laterThan, CancellationToken cancellationToken)
    {
        var filter = Builders<TestFixture>.Filter.Eq(x => x.ProjectId, projectId) & Builders<TestFixture>.Filter.Eq(x => x.ProjectVersion, projectVersion) &
             Builders<TestFixture>.Filter.Gt(x => x.LastUpdated, laterThan);
        var fixtures = await fixturesCollection.FindAsync(filter, FindOptions, cancellationToken);
        return await fixtures.ToListAsync();       
    }

    /// <inheritdoc/>  
    public async Task AddFixtureAsync(string projectId, string projectVersion, TestFixture fixture, CancellationToken cancellationToken)
    {
        Guard.Argument(fixture).NotNull();
        var exists = (await FindByIdAsync(projectId, projectVersion, fixture.FixtureId, cancellationToken)) != null;
        if(exists)
        {
            throw new InvalidOperationException($"Fixture with Id : {fixture.FixtureId} alreadye exists for Project : {fixture.ProjectId} , Version : {fixture.ProjectVersion}");
        }
        fixture.ProjectId = projectId;
        fixture.ProjectVersion = projectVersion;
        await fixturesCollection.InsertOneAsync(fixture, InsertOneOptions, cancellationToken);
        await referencesRepository.AddFixtureAsync(projectId, projectVersion, fixture.FixtureId);
        logger.LogInformation("Fixture {0} was added.", fixture);
    }

    /// <inheritdoc/>  
    public async Task AddFixturesAsync(string projectId, string projectVersion, IEnumerable<TestFixture> fixtures, CancellationToken cancellationToken)
    {
        Guard.Argument(fixtures).NotNull();
        if(fixtures.Any())
        {
            foreach (var fixture in fixtures)
            {
                fixture.ProjectId = projectId;
                fixture.ProjectVersion = projectVersion;
            }
            //we don't update entry in the  references file as this method is used when cloning a project version and references file will already have required entries
            await fixturesCollection.InsertManyAsync(fixtures, new InsertManyOptions(), cancellationToken).ConfigureAwait(false);          
        }      
    }

    /// <inheritdoc/>  
    public async Task UpdateFixtureAsync(string projectId, string projectVersion, TestFixture fixture, CancellationToken cancellationToken)
    {       
        var filter = Builders<TestFixture>.Filter.And(Builders<TestFixture>.Filter.Eq(x => x.ProjectId, projectId),
                             Builders<TestFixture>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                             Builders<TestFixture>.Filter.Eq(x => x.FixtureId, fixture.FixtureId));
        var updateDefinition = Builders<TestFixture>.Update
          .Set(t => t.DisplayName, fixture.DisplayName)
          .Set(t => t.Order, fixture.Order)
          .Set(t => t.IsMuted, fixture.IsMuted)
          .Set(t => t.Description, fixture.Description)
          .Set(t => t.PostDelay, fixture.PostDelay)
          .Set(t => t.DelayFactor, fixture.DelayFactor)
          .Set(t => t.Tags, fixture.Tags)
          .Set(t => t.ControlsUsed, fixture.ControlsUsed)
          .Set(t => t.PrefabsUsed, fixture.PrefabsUsed)
          .Set(t => t.LastUpdated, DateTime.UtcNow)
          .Inc(t => t.Revision, 1);
        var result =  await fixturesCollection.FindOneAndUpdateAsync(filter, updateDefinition, FindOneAndUpdateOptions, cancellationToken);
        logger.LogInformation("Fixture with Id : {0} was updated to {1}", fixture.FixtureId, result);
    }

    /// <inheritdoc/>  
    public async Task DeleteFixtureAsync(string projectId, string projectVersion, string fixtureId, CancellationToken cancellationToken)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull();
        Guard.Argument(fixtureId, nameof(fixtureId)).NotNull();

        var fixtureFilter = Builders<TestFixture>.Filter.And(Builders<TestFixture>.Filter.Eq(x => x.ProjectId, projectId),
                             Builders<TestFixture>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                             Builders<TestFixture>.Filter.Eq(x => x.FixtureId, fixtureId));

        var fixtureUpdateDefinition = Builders<TestFixture>.Update
            .Set(t => t.IsDeleted, true)         
            .Set(t => t.LastUpdated, DateTime.UtcNow)
            .Inc(t => t.Revision, 1);

        await fixturesCollection.FindOneAndUpdateAsync(fixtureFilter, fixtureUpdateDefinition, FindOneAndUpdateOptions, cancellationToken);
        await referencesRepository.DeleteFixtureAsync(projectId, projectVersion, fixtureId);

        var testFilter = Builders<TestCase>.Filter.And(Builders<TestCase>.Filter.Eq(x => x.ProjectId, projectId),
                             Builders<TestCase>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                             Builders<TestCase>.Filter.Eq(x => x.FixtureId, fixtureId));

        var testUpdateDefinition = Builders<TestCase>.Update
            .Set(t => t.IsDeleted, true)
            .Set(t => t.LastUpdated, DateTime.UtcNow)
            .Inc(t => t.Revision, 1);

       await testsCollection.UpdateManyAsync(testFilter, testUpdateDefinition, new UpdateOptions(), cancellationToken);
       logger.LogInformation("Fixture with Id : '{0}' was deleted from version : '{0}' of project : '{1}'", fixtureId, projectVersion, projectId);
    }
    
}
