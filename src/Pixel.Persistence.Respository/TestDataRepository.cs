using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Pixel.Persistence.Core.Models;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using Pixel.Persistence.Respository.Interfaces;
using System.Linq;
using MongoDB.Driver.Core.Misc;

namespace Pixel.Persistence.Respository
{
    public class TestDataRepository : ITestDataRepository
    {
        private readonly ILogger logger;
        private readonly IMongoCollection<TestDataSource> testDataCollection;
        private readonly IMongoCollection<TestCase> testsCollection;
        private readonly IReferencesRepository referencesRepository;

        private static readonly InsertOneOptions InsertOneOptions = new InsertOneOptions();
        private static readonly FindOptions<TestDataSource> FindOptions = new FindOptions<TestDataSource>();
        private static readonly FindOneAndUpdateOptions<TestDataSource> FindOneAndUpdateOptions = new FindOneAndUpdateOptions<TestDataSource>();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbSettings"></param>
        public TestDataRepository(ILogger<TestFixtureRepository> logger, IMongoDbSettings dbSettings, IReferencesRepository referencesRepository)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            this.testDataCollection = database.GetCollection<TestDataSource>(dbSettings.TestDataCollectionName);
            this.testsCollection = database.GetCollection<TestCase>(dbSettings.TestsCollectionName);
            this.referencesRepository = Guard.Argument(referencesRepository, nameof(referencesRepository)).NotNull().Value;
        }

        /// <inheritdoc/>  
        public async Task<TestDataSource> FindByIdAsync(string projectId, string projectVersion, string dataSSourceId, CancellationToken cancellationToken)
        {
            Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
            Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
            Guard.Argument(dataSSourceId, nameof(dataSSourceId)).NotNull().NotEmpty();

            var filter = Builders<TestDataSource>.Filter.And(Builders<TestDataSource>.Filter.Eq(x => x.ProjectId, projectId),
                                Builders<TestDataSource>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                                Builders<TestDataSource>.Filter.Eq(x => x.DataSourceId, dataSSourceId));
            return (await testDataCollection.FindAsync(filter, FindOptions, cancellationToken)).FirstOrDefault();
        }

        /// <inheritdoc/>  
        public async Task<TestDataSource> FindByNameAsync(string projectId, string projectVersion, string name, CancellationToken cancellationToken)
        {
            Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
            Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();

            var filter = Builders<TestDataSource>.Filter.And(Builders<TestDataSource>.Filter.Eq(x => x.ProjectId, projectId),
                                Builders<TestDataSource>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                                Builders<TestDataSource>.Filter.Eq(x => x.Name, name));
            return (await testDataCollection.FindAsync(filter, FindOptions, cancellationToken)).FirstOrDefault();
        }

        /// <inheritdoc/>  
        public async Task<IEnumerable<TestDataSource>> GetDataSourcesAsync(string projectId, string projectVersion, DateTime laterThan, CancellationToken cancellationToken)
        {
            Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
            Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();

            var filter = Builders<TestDataSource>.Filter.And(Builders<TestDataSource>.Filter.Eq(x => x.ProjectId, projectId),
                Builders<TestDataSource>.Filter.Eq(x => x.ProjectVersion, projectVersion)) &
                Builders<TestDataSource>.Filter.Gt(x => x.LastUpdated, laterThan);
            var dataSources = await testDataCollection.FindAsync(filter, FindOptions, cancellationToken);
            return await dataSources.ToListAsync();
        }

        /// <inheritdoc/>  
        public async Task AddDataSourceAsync(string projectId, string projectVersion, string groupName, TestDataSource dataSource, CancellationToken cancellationToken)
        {
            Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
            Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
            Guard.Argument(groupName, nameof(groupName)).NotNull().NotEmpty();
            Guard.Argument(dataSource, nameof(dataSource)).NotNull();

            var exists = await FindByIdAsync(projectId, projectVersion, dataSource.DataSourceId, cancellationToken) != null;
            if (exists)
            {
                throw new InvalidOperationException($"Test Data Source with Id : {dataSource.DataSourceId} already exists for Project : {dataSource.ProjectId} , Version : {dataSource.ProjectVersion}");
            }
            dataSource.ProjectId = projectId;
            dataSource.ProjectVersion = projectVersion;
            await testDataCollection.InsertOneAsync(dataSource, InsertOneOptions, cancellationToken).ConfigureAwait(false);
            await referencesRepository.AddDataSourceToGroupAsync(projectId, projectVersion, groupName, dataSource.DataSourceId);
            logger.LogInformation("Test Data Source {0} was added  to version {1} of project {2}", dataSource.Name, projectVersion, projectId);
        }

        /// <inheritdoc/>  
        public async Task AddDataSourcesAsync(string projectId, string projectVersion, IEnumerable<TestDataSource> dataSources, CancellationToken cancellationToken)
        {
            Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
            Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
            Guard.Argument(dataSources, nameof(dataSources)).NotNull().NotEmpty();

            foreach (var dataSource in dataSources)
            {
                dataSource.ProjectId = projectId;
                dataSource.ProjectVersion = projectVersion;
            }
            //we don't update entry in the  references file as this method is used when cloning a project version and references file will already have required entries
            await testDataCollection.InsertManyAsync(dataSources, new InsertManyOptions(), cancellationToken).ConfigureAwait(false);
            logger.LogInformation("{0} test data sources were added to version {1} of project {2}", dataSources.Count(), projectVersion, projectId);
        }

        /// <inheritdoc/>  
        public async Task UpdateDataSourceAsync(string projectId, string projectVersion, TestDataSource dataSource, CancellationToken cancellationToken)
        {
            Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
            Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
            Guard.Argument(dataSource, nameof(dataSource)).NotNull();

            var testDataSource = await FindByIdAsync(projectId, projectVersion, dataSource.DataSourceId, cancellationToken);

            var filter = Builders<TestDataSource>.Filter.And(Builders<TestDataSource>.Filter.Eq(x => x.ProjectId, projectId),
                                 Builders<TestDataSource>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                                 Builders<TestDataSource>.Filter.Eq(x => x.DataSourceId, dataSource.DataSourceId));
            var updateDefinition = Builders<TestDataSource>.Update
              .Set(t => t.Name, dataSource.Name)
              .Set(t => t.ScriptFile, dataSource.ScriptFile)
              .Set(t => t.MetaData, dataSource.MetaData)
              .Set(t => t.DataSource, dataSource.DataSource)
              .Set(t => t.LastUpdated, DateTime.UtcNow)
              .Inc(t => t.Revision, 1);
            var result = await testDataCollection.FindOneAndUpdateAsync(filter, updateDefinition, FindOneAndUpdateOptions, cancellationToken);
            logger.LogInformation("Test data source : {0} was updated for version {1} of project {2}", testDataSource.Name, projectVersion, projectId);
        }

        /// <inheritdoc/>  
        public async Task DeleteDataSourceAsync(string projectId, string projectVersion, string dataSourceId, CancellationToken cancellationToken)
        {
            Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
            Guard.Argument(projectVersion, nameof(projectVersion)).NotNull();
            Guard.Argument(dataSourceId, nameof(dataSourceId)).NotNull();

            var testDataSource = await FindByIdAsync(projectId, projectVersion, dataSourceId, cancellationToken);

            //Check if any of the test case is using the data source
            var filter = Builders<TestCase>.Filter.Eq(x => x.TestDataId, dataSourceId) 
                        & Builders<TestCase>.Filter.Eq(x => x.ProjectId, projectId)
                        & Builders<TestCase>.Filter.Eq(x => x.ProjectVersion, projectVersion);
            long count = await this.testsCollection.CountDocumentsAsync(filter);
            if (count > 0)
            {
                throw new InvalidOperationException($"Test data source : {testDataSource.Name} is in use by {count} test cases");
            }

            var fixtureFilter = Builders<TestDataSource>.Filter.And(Builders<TestDataSource>.Filter.Eq(x => x.ProjectId, projectId),
                                 Builders<TestDataSource>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                                 Builders<TestDataSource>.Filter.Eq(x => x.DataSourceId, dataSourceId));

            var updateDefinition = Builders<TestDataSource>.Update
                .Set(t => t.IsDeleted, true)
                .Set(t => t.LastUpdated, DateTime.UtcNow)
                .Inc(t => t.Revision, 1);

            await testDataCollection.FindOneAndUpdateAsync(fixtureFilter, updateDefinition, FindOneAndUpdateOptions, cancellationToken);
            await referencesRepository.DeleteDataSourceAsync(projectId, projectVersion, dataSourceId);
            logger.LogInformation("Test data souce {0} was marked deleted for version {1} of project {2}", testDataSource.Name, projectVersion, projectId);
        }

    }
}
