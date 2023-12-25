using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository.Extensions;
using Pixel.Persistence.Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class PrefabsRepository : IPrefabsRepository
    {
        private readonly ILogger logger;
        private readonly IMongoCollection<PrefabProject> prefabsCollection;     
        private readonly IPrefabFilesRepository prefabFilesRepossitory;
        private readonly IReferencesRepository referencesRepository;
        private static readonly InsertOneOptions InsertOneOptions = new InsertOneOptions();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="dbSettings"></param>
        public PrefabsRepository(ILogger<PrefabsRepository> logger, IMongoDbSettings dbSettings,
            IPrefabFilesRepository prefabFilesRepository, IReferencesRepository referencesRepository)
        {
            Guard.Argument(dbSettings, nameof(dbSettings)).NotNull();
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;           
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            this.prefabsCollection = database.GetCollection<PrefabProject>(dbSettings.PrefabProjectsCollectionName);
            this.prefabFilesRepossitory = Guard.Argument(prefabFilesRepository, nameof(prefabFilesRepository)).NotNull().Value;
            this.referencesRepository = Guard.Argument(referencesRepository, nameof(referencesRepository)).NotNull().Value;

        }

        /// <inheritdoc/>
        public async Task<PrefabProject> FindByIdAsync(string prefabId, CancellationToken cancellationToken)
        {
            return await prefabsCollection.FindFirstOrDefaultAsync(x => x.ProjectId.Equals(prefabId), cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PrefabProject> FindByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await prefabsCollection.FindFirstOrDefaultAsync(x => x.Name.Equals(name), cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PrefabProject>> FindAllAsync(CancellationToken cancellationToken)
        {
            var projects = await prefabsCollection.FindAsync(Builders<PrefabProject>.Filter.Empty,
                null, cancellationToken);
            return projects.ToList();
        }

        /// <inheritdoc/>
        public async Task AddPrefabAsync(PrefabProject prefabProject, CancellationToken cancellationToken)
        {
            Guard.Argument(prefabProject).NotNull();
            var exists = (await FindByIdAsync(prefabProject.ProjectId, cancellationToken)) != null;
            if (exists)
            {
                throw new InvalidOperationException($"Project with Id : {prefabProject.ProjectId} already exists");
            }
            await prefabsCollection.InsertOneAsync(prefabProject, InsertOneOptions, cancellationToken).ConfigureAwait(false);
            logger.LogInformation("Prefab project {0} was added.", prefabProject);
        }

        /// <inheritdoc/>
        public async Task AddPrefabVersionAsync(string prefabId, VersionInfo newVersion, VersionInfo cloneFrom, CancellationToken cancellationToken)
        {
            var filter = Builders<PrefabProject>.Filter.Eq(x => x.ProjectId, prefabId)
                          & Builders<PrefabProject>.Filter.ElemMatch(x => x.AvailableVersions, Builders<VersionInfo>.Filter.Eq(x => x.Version, newVersion.Version));
            var projectVersions = (await this.prefabsCollection.FindAsync<List<VersionInfo>>(filter, new FindOptions<PrefabProject, List<VersionInfo>>()
            {
                Projection = Builders<PrefabProject>.Projection.Expression(u => u.AvailableVersions)
            }))
            .ToList().FirstOrDefault();
            
            if (projectVersions?.Contains(newVersion) ?? false)
            {
                throw new InvalidOperationException($"Version {newVersion} already exists for prefab {prefabId}");
            }

            var prefabReferences = await this.referencesRepository.GetProjectReferences(prefabId, cloneFrom.Version.ToString());
            prefabReferences.Id = string.Empty;
            await this.referencesRepository.AddProjectReferences(prefabId, newVersion.ToString(), prefabReferences);

            
            await foreach (var file in this.prefabFilesRepossitory.GetFilesAsync(prefabId, cloneFrom.ToString(), new string[] { }))
            {
                //when creating a copy from previous version, we don't want dll and pdb files to be brough in to new version.                
                if(Path.GetExtension(file.FileName).Equals(".dll") || Path.GetExtension(file.FileName).Equals(".pdb"))
                {                  
                    continue;
                }
                await this.prefabFilesRepossitory.AddOrUpdateFileAsync(prefabId, newVersion.ToString(), file);
            }

            filter = Builders<PrefabProject>.Filter.Eq(x => x.ProjectId, prefabId);
            var push = Builders<PrefabProject>.Update.Push(t => t.AvailableVersions, newVersion)
                .Set(t => t.LastUpdated, DateTime.UtcNow)
                .Inc(t => t.Revision, 1);
            await this.prefabsCollection.UpdateOneAsync(filter, push);
        }

        /// <inheritdoc/>
        public async Task UpdatePrefabVersionAsync(string prefabId, VersionInfo prefabVersion, CancellationToken cancellationToken)
        {
            var filter = Builders<PrefabProject>.Filter.Eq(x => x.ProjectId, prefabId)
                         & Builders<PrefabProject>.Filter.ElemMatch(x => x.AvailableVersions, Builders<VersionInfo>.Filter.Eq(x => x.Version, prefabVersion.Version));
            var prefabVersions = (await this.prefabsCollection.FindAsync<List<VersionInfo>>(filter, new FindOptions<PrefabProject, List<VersionInfo>>()
            {
                Projection = Builders<PrefabProject>.Projection.Expression(u => u.AvailableVersions)
            })).ToList().FirstOrDefault();

            if (prefabVersions?.Contains(prefabVersion) ?? false)
            {
                if(!prefabVersion.IsActive && prefabVersion.PublishedOn is null)
                {
                    prefabVersion.PublishedOn = DateTime.UtcNow;
                }
                var update = Builders<PrefabProject>.Update.Set(x => x.AvailableVersions.FirstMatchingElement(), prefabVersion)
                     .Set(t => t.LastUpdated, DateTime.UtcNow)
                     .Inc(t => t.Revision, 1); ;
                await this.prefabsCollection.UpdateOneAsync(filter, update);
                logger.LogInformation("Project version {0} was updated for project : {1}", prefabVersion, prefabId);
                return;
            }
            throw new InvalidOperationException($"Version {prefabVersion} doesn't exist on prefab {prefabId}");
        }

        ///<inheritdoc/>
        public async Task<bool> IsPrefabDeleted(string prefabId)
        {
            Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
            var filter = Builders<PrefabProject>.Filter.Eq(x => x.ProjectId, prefabId) &
                Builders<PrefabProject>.Filter.Eq(x => x.IsDeleted, true);
            long count = await this.prefabsCollection.CountDocumentsAsync(filter, new CountOptions() { Limit = 1 });
            return count > 0;
        }

        /// <inheritdoc/>
        public async Task DeletePrefabAsync(string prefabId)
        {
            Guard.Argument(prefabId, nameof(prefabId)).NotNull();
            if(await this.referencesRepository.IsPrefabInUse(prefabId))
            {
                throw new InvalidOperationException("Prefab is in use across one or more projects");
            }
            var updateFilter = Builders<PrefabProject>.Filter.Eq(x => x.ProjectId, prefabId);
            var updateDefinition = Builders<PrefabProject>.Update.Set(x => x.IsDeleted, true)
                .Set(x => x.LastUpdated, DateTime.UtcNow)
                .Inc(t => t.Revision, 1);
            await this.prefabsCollection.FindOneAndUpdateAsync<PrefabProject>(updateFilter, updateDefinition);
        }

        /// <inheritdoc/>
        public async Task DeleteAllPrefabsForApplicationAsync(string applicationId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull();
            var updateFilter = Builders<PrefabProject>.Filter.Eq(x => x.ApplicationId, applicationId);
            var updateDefinition = Builders<PrefabProject>.Update.Set(x => x.IsDeleted, true)
               .Set(x => x.LastUpdated, DateTime.UtcNow)
               .Inc(t => t.Revision, 1);
            var result = await this.prefabsCollection.UpdateManyAsync(updateFilter, updateDefinition);
            logger.LogInformation("{0} prefabs were marked deleted for application with Id : '{1}'", result.ModifiedCount, applicationId);
        }
    }
}
