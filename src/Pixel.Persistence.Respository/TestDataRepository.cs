using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Pixel.Persistence.Core.Models;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using Pixel.Persistence.Respository.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;

namespace Pixel.Persistence.Respository
{
    public class TestDataRepository : ITestDataRepository
    {

        private readonly ILogger logger;
        private readonly IMongoCollection<TestDataSource> testDataCollection;

        private static readonly InsertOneOptions InsertOneOptions = new InsertOneOptions();
        private static readonly FindOptions<TestDataSource> FindOptions = new FindOptions<TestDataSource>();
        private static readonly FindOneAndUpdateOptions<TestDataSource> FindOneAndUpdateOptions = new FindOneAndUpdateOptions<TestDataSource>();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbSettings"></param>
        public TestDataRepository(ILogger<TestFixtureRepository> logger, IMongoDbSettings dbSettings)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            testDataCollection = database.GetCollection<TestDataSource>(dbSettings.TestDataCollectionName);
        }

        /// <inheritdoc/>  
        public async Task<TestDataSource> FindByIdAsync(string projectId, string projectVersion, string fixtureId, CancellationToken cancellationToken)
        {
            var filter = Builders<TestDataSource>.Filter.And(Builders<TestDataSource>.Filter.Eq(x => x.ProjectId, projectId),
                                Builders<TestDataSource>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                                Builders<TestDataSource>.Filter.Eq(x => x.DataSourceId, fixtureId));
            return (await testDataCollection.FindAsync(filter, FindOptions, cancellationToken)).FirstOrDefault();
        }

        /// <inheritdoc/>  
        public async Task<TestDataSource> FindByNameAsync(string projectId, string projectVersion, string name, CancellationToken cancellationToken)
        {
            var filter = Builders<TestDataSource>.Filter.And(Builders<TestDataSource>.Filter.Eq(x => x.ProjectId, projectId),
                                Builders<TestDataSource>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                                Builders<TestDataSource>.Filter.Eq(x => x.Name, name));
            return (await testDataCollection.FindAsync(filter, FindOptions, cancellationToken)).FirstOrDefault();
        }

        /// <inheritdoc/>  
        public async Task<IEnumerable<TestDataSource>> GetDataSourcesAsync(string projectId, string projectVersion, CancellationToken cancellationToken)
        {
            var filter = Builders<TestDataSource>.Filter.And(Builders<TestDataSource>.Filter.Eq(x => x.ProjectId, projectId),
                Builders<TestDataSource>.Filter.Eq(x => x.ProjectVersion, projectVersion));
            var dataSources = await testDataCollection.FindAsync(filter, FindOptions, cancellationToken);
            return await dataSources.ToListAsync();
        }

        /// <inheritdoc/>  
        public async Task AddDataSourceAsync(string projectId, string projectVersion, TestDataSource dataSource, CancellationToken cancellationToken)
        {
            Guard.Argument(dataSource).NotNull();
            var exists = await FindByIdAsync(projectId, projectVersion, dataSource.DataSourceId, cancellationToken) != null;
            if (exists)
            {
                throw new InvalidOperationException($"Test Data Source with Id : {dataSource.DataSourceId} alreadye exists for Project : {dataSource.ProjectId} , Version : {dataSource.ProjectVersion}");
            }
            dataSource.ProjectId = projectId;
            dataSource.ProjectVersion = projectVersion;
            await testDataCollection.InsertOneAsync(dataSource, InsertOneOptions, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Test Data Source {0} was added.", dataSource);
        }

        /// <inheritdoc/>  
        public async Task AddDataSourcesAsync(string projectId, string projectVersion, IEnumerable<TestDataSource> dataSources, CancellationToken cancellationToken)
        {
            Guard.Argument(dataSources).NotNull();
            if (dataSources.Any())
            {
                foreach (var dataSource in dataSources)
                {
                    dataSource.ProjectId = projectId;
                    dataSource.ProjectVersion = projectVersion;
                }

                await testDataCollection.InsertManyAsync(dataSources, new InsertManyOptions(), cancellationToken).ConfigureAwait(false);
            }
        }


        /// <inheritdoc/>  
        public async Task UpdateDataSourceAsync(string projectId, string projectVersion, TestDataSource dataSource, CancellationToken cancellationToken)
        {
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
            logger.LogInformation("Fixture with Id : {0} was updated to {1}", dataSource.DataSourceId, result);
        }

        /// <inheritdoc/>  
        public async Task DeleteDataSourceAsync(string projectId, string projectVersion, string dataSourceId, CancellationToken cancellationToken)
        {
            Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
            Guard.Argument(projectVersion, nameof(projectVersion)).NotNull();
            Guard.Argument(dataSourceId, nameof(dataSourceId)).NotNull();

            var fixtureFilter = Builders<TestDataSource>.Filter.And(Builders<TestDataSource>.Filter.Eq(x => x.ProjectId, projectId),
                                 Builders<TestDataSource>.Filter.Eq(x => x.ProjectVersion, projectVersion),
                                 Builders<TestDataSource>.Filter.Eq(x => x.DataSourceId, dataSourceId));

            var updateDefinition = Builders<TestDataSource>.Update
                .Set(t => t.IsDeleted, true)
                .Set(t => t.LastUpdated, DateTime.UtcNow)
                .Inc(t => t.Revision, 1);

            await testDataCollection.FindOneAndUpdateAsync(fixtureFilter, updateDefinition, FindOneAndUpdateOptions, cancellationToken);
        }

    }
}
