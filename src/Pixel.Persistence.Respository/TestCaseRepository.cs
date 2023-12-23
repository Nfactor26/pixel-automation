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

public class TestCaseRepository : ITestCaseRepository
{
    private readonly ILogger logger;
    private readonly IMongoCollection<TestCase> testsCollection;
    private readonly IMongoCollection<TestFixture> fixturesCollection;

    private static readonly InsertOneOptions InsertOneOptions = new InsertOneOptions();
    private static readonly FindOptions<TestCase> TestCasFindOptions = new FindOptions<TestCase>();
    private static readonly FindOneAndUpdateOptions<TestCase> FindOneAndUpdateTestCaseOptions = new FindOneAndUpdateOptions<TestCase>();
    private static readonly FindOneAndUpdateOptions<TestFixture> FindOneAndUpdateFixtureOptions = new FindOneAndUpdateOptions<TestFixture>();


    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="dbSettings"></param>
    public TestCaseRepository(ILogger<TestFixtureRepository> logger, IMongoDbSettings dbSettings)
    {
        this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
        var client = new MongoClient(dbSettings.ConnectionString);
        var database = client.GetDatabase(dbSettings.DatabaseName);
        this.testsCollection = database.GetCollection<TestCase>(dbSettings.TestsCollectionName);
        this.fixturesCollection = database.GetCollection<TestFixture>(dbSettings.FixturesCollectionName);
    }

    /// <inheritdoc/>  
    public async Task<TestCase> FindByIdAsync(string projectId, string projectVersion, string testId, CancellationToken cancellationToken)
    {
        var filter = Builders<TestCase>.Filter.And(Builders<TestCase>.Filter.Eq(x => x.ProjectId, projectId),
                             Builders<TestCase>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                             Builders<TestCase>.Filter.Eq(x => x.TestCaseId, testId));
        return (await testsCollection.FindAsync(filter, TestCasFindOptions, cancellationToken)).FirstOrDefault();
    }

    /// <inheritdoc/>  
    public async Task<TestCase> FindByNameAsync(string projectId, string projectVersion, string name, CancellationToken cancellationToken)
    {
        var filter = Builders<TestCase>.Filter.And(Builders<TestCase>.Filter.Eq(x => x.ProjectId, projectId),
                            Builders<TestCase>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                            Builders<TestCase>.Filter.Eq(x => x.DisplayName, name));
        return (await testsCollection.FindAsync(filter, TestCasFindOptions, cancellationToken)).FirstOrDefault();
    }

    /// <inheritdoc/>  
    public async Task<IEnumerable<TestCase>> GetTestCasesAsync(string projectId, string projectVersion, DateTime laterThan, CancellationToken cancellationToken)
    {
        var filter = Builders<TestCase>.Filter.Eq(x => x.ProjectId, projectId) & Builders<TestCase>.Filter.Eq(x => x.ProjectVersion, projectVersion) &
            Builders<TestCase>.Filter.Eq(x => x.IsDeleted, false) & Builders<TestCase>.Filter.Gt(x => x.LastUpdated, laterThan);
        var tests = await testsCollection.FindAsync(filter, TestCasFindOptions, cancellationToken);
        return await tests.ToListAsync();
    }

    /// <inheritdoc/>  
    public async Task<IEnumerable<TestCase>> GetTestCasesAsync(string projectId, string projectVersion, string fixtureId, DateTime laterThan, CancellationToken cancellationToken)
    {
        var filter = Builders<TestCase>.Filter.Eq(x => x.ProjectId, projectId) & Builders<TestCase>.Filter.Eq(x => x.ProjectVersion, projectVersion) &
            Builders<TestCase>.Filter.Eq(x => x.FixtureId, fixtureId) & Builders<TestCase>.Filter.Eq(x => x.IsDeleted, false) &
            Builders<TestCase>.Filter.Gt(x => x.LastUpdated, laterThan);
        var tests = await testsCollection.FindAsync(filter, TestCasFindOptions, cancellationToken);
        return await tests.ToListAsync();
    }

    /// <inheritdoc/>  
    public async Task AddTestCaseAsync(string projectId, string projectVersion, TestCase testCase, CancellationToken cancellationToken)
    {
        Guard.Argument(testCase).NotNull();
        var exists = (await FindByIdAsync(projectId, projectVersion, testCase.TestCaseId, cancellationToken)) != null;
        if (exists)
        {
            throw new InvalidOperationException($"TestCase with Id : {testCase.TestCaseId} alreadye exists for Project : {testCase.ProjectId} , Version : {testCase.ProjectVersion}");
        }
        testCase.ProjectId = projectId;
        testCase.ProjectVersion = projectVersion;
        await testsCollection.InsertOneAsync(testCase, InsertOneOptions, cancellationToken).ConfigureAwait(false);
        await AddTestCaseToFixture(testCase, cancellationToken); //Add an entry to the fixture for this test case
        logger.LogInformation("TestCase {0} was added.", testCase);
    }

    async Task AddTestCaseToFixture(TestCase testCase, CancellationToken cancellationToken)
    {
        var filter = Builders<TestFixture>.Filter.And(Builders<TestFixture>.Filter.Eq(x => x.ProjectId, testCase.ProjectId),
                           Builders<TestFixture>.Filter.Eq(x => x.ProjectVersion, testCase.ProjectVersion),
                           Builders<TestFixture>.Filter.Eq(x => x.FixtureId, testCase.FixtureId));

        var updateDefinition = Builders<TestFixture>.Update.Push(t => t.TestCases, testCase.TestCaseId)
             .Set(t => t.LastUpdated, DateTime.UtcNow)
             .Inc(t => t.Revision, 1);


        var result = await fixturesCollection.FindOneAndUpdateAsync(filter, updateDefinition, FindOneAndUpdateFixtureOptions, cancellationToken);
        logger.LogInformation("Test Case : '{0}' was added to Fixture : '{1}' for version : '{2}' of project : '{3}'. Result is : '{4}'",
            testCase.TestCaseId, testCase.FixtureId, testCase.ProjectId, testCase.ProjectVersion, result);
    }

    /// <inheritdoc/>  
    public async Task AddTestCasesAsync(string projectId, string projectVersion, IEnumerable<TestCase> tests, CancellationToken cancellationToken)
    {
        Guard.Argument(tests).NotNull();
        if (tests.Any())
        {
            foreach (var test in tests)
            {
                test.ProjectId = projectId;
                test.ProjectVersion = projectVersion;
            }

            //we don't update fixture to add entry as this method is called when creating a new revision of project and fixture will already have an entry
            await testsCollection.InsertManyAsync(tests, new InsertManyOptions(), cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>  
    public async Task UpdateTestCaseAsync(string projectId, string projectVersion, TestCase testCase, CancellationToken cancellationToken)
    {      
        var filter = Builders<TestCase>.Filter.Eq(f => f.ProjectId, projectId) & Builders<TestCase>.Filter.Eq(f => f.ProjectVersion, projectVersion)
            & Builders<TestCase>.Filter.Eq(f => f.TestCaseId, testCase.TestCaseId);
        var updateDefinition = Builders<TestCase>.Update
          .Set(t => t.DisplayName, testCase.DisplayName)
          .Set(t => t.Order, testCase.Order)
          .Set(t => t.IsMuted, testCase.IsMuted)
          .Set(t => t.TestDataId, testCase.TestDataId)
          .Set(t => t.PostDelay, testCase.PostDelay)
          .Set(t => t.DelayFactor, testCase.DelayFactor)
          .Set(t => t.Priority, testCase.Priority)
          .Set(t => t.Description, testCase.Description)
          .Set(t => t.Tags, testCase.Tags)
          .Set(t => t.ControlsUsed, testCase.ControlsUsed)
          .Set(t => t.PrefabsUsed, testCase.PrefabsUsed)
          .Set(t => t.LastUpdated, DateTime.UtcNow)
          .Inc(t => t.Revision, 1);
       
        var result = await testsCollection.FindOneAndUpdateAsync(filter, updateDefinition, FindOneAndUpdateTestCaseOptions, cancellationToken);
        logger.LogInformation("TestCase with Id : {0} was updated to {1}", testCase.TestCaseId, result);
    }

    /// <inheritdoc/>  
    public async Task DeleteTestCaseAsync(string projectId, string projectVersion, string testCaseId, CancellationToken cancellationToken)
    {
        var filter = Builders<TestCase>.Filter.And(Builders<TestCase>.Filter.Eq(x => x.ProjectId, projectId),
                              Builders<TestCase>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                              Builders<TestCase>.Filter.Eq(x => x.TestCaseId, testCaseId));        

        var updateDefinition = Builders<TestCase>.Update
            .Set(t => t.IsDeleted, true)
            .Set(t => t.LastUpdated, DateTime.UtcNow)
            .Inc(t => t.Revision, 1);

        await testsCollection.FindOneAndUpdateAsync(filter, updateDefinition, FindOneAndUpdateTestCaseOptions, cancellationToken);
        await DeleteTestCaseFromFixtureAsync((await FindByIdAsync(projectId, projectVersion, testCaseId, cancellationToken)), cancellationToken);
        logger.LogInformation("TestCase with Id : {0} was deleted", testCaseId);
    }

    async Task DeleteTestCaseFromFixtureAsync(TestCase testCase, CancellationToken cancellationToken)
    {
        var filter = Builders<TestFixture>.Filter.And(Builders<TestFixture>.Filter.Eq(x => x.ProjectId, testCase.ProjectId),
                             Builders<TestFixture>.Filter.Eq(x => x.ProjectVersion, testCase.ProjectVersion),
                             Builders<TestFixture>.Filter.Eq(x => x.FixtureId, testCase.FixtureId));
       
        var updateDefinition = Builders<TestFixture>.Update.Pull(t => t.TestCases, testCase.TestCaseId)
           .Set(t => t.LastUpdated, DateTime.UtcNow)
           .Inc(t => t.Revision, 1);

        var result = await fixturesCollection.FindOneAndUpdateAsync(filter, updateDefinition, FindOneAndUpdateFixtureOptions, cancellationToken);
        logger.LogInformation("Test Case : '{0}' was removed from Fixture : '{1}' for version : '{2}' of project : '{3}'. Result is : '{4}'",
            testCase.TestCaseId, testCase.FixtureId, testCase.ProjectId, testCase.ProjectVersion, result);
    }

}
