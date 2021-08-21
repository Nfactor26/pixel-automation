using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class PrefabRepository : IPrefabRepository
    {
        private readonly string prefabFile = "PrefabFile";
        private readonly string prefabDataFiles = "PrefabDataFiles";
    
        private readonly ILogger logger;
        private readonly IGridFSBucket bucket;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dbSettings"></param>
        public PrefabRepository(ILogger<ProjectRepository> logger, IMongoDbSettings dbSettings)
        {
            Guard.Argument(dbSettings).NotNull();
            this.logger = Guard.Argument(logger).NotNull().Value;

            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            bucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = dbSettings.PrefabsBucketName
            });
        }

        ///<inheritdoc/>
        public async Task AddOrUpdatePrefabAsync(PrefabMetaData prefab, string fileName, byte[] fileData)
        {
            Guard.Argument(prefab).NotNull();          
           
            switch (prefab.Type)
            {
                case "PrefabFile":                   
                    await bucket.UploadFromBytesAsync(fileName, fileData, new GridFSUploadOptions()
                    {
                        Metadata = new MongoDB.Bson.BsonDocument()
                         {
                            {"prefabId" , prefab.PrefabId},
                            {"applicationId", prefab.ApplicationId},
                            {"type", prefab.Type}
                         }
                    });
                    break;

                case "PrefabDataFiles":
                    var prefabIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["prefabId"], prefab.PrefabId);                   
                    var prefabVersionFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["version"], prefab.Version);
                    var filter = Builders<GridFSFileInfo>.Filter.And(prefabIdFilter, prefabVersionFilter);
                    var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
                    var options = new GridFSFindOptions
                    {
                        Limit = 1,
                        Sort = sort
                    };
                    using (var cursor = await bucket.FindAsync(filter, options))
                    {
                        var fileInfo = (await cursor.ToListAsync()).FirstOrDefault();
                        if (fileInfo?.Metadata["isDeployed"].AsBoolean ?? false)
                        {
                            throw new InvalidOperationException($"Version : {prefab.Version} for prefab with id : {prefab.PrefabId} is deployed. Can't update a deployed version");
                        }
                    }

                    await bucket.UploadFromBytesAsync(fileName, fileData, new GridFSUploadOptions()
                    {
                        Metadata = new MongoDB.Bson.BsonDocument()
                        {
                            {"prefabId" , prefab.PrefabId},
                            {"applicationId", prefab.ApplicationId},
                            {"version", prefab.Version},
                            {"type", prefab.Type},
                            {"isActive", prefab.IsActive},
                            {"isDeployed", prefab.IsDeployed}
                        }
                    });
                    break;
                default:
                    throw new ArgumentException($"type : {prefab.Type} is not supported. Valid values are PrefabFile file and PrefabDataFiles for contents of a prefab version (process, template, scripts, etc)");
            }
        }

        ///<inheritdoc/>
        public async Task<byte[]> GetPrefabFileAsync(string projectId)
        {
            Guard.Argument(projectId).NotNull().NotEmpty();
            var projectIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["prefabId"], projectId);
            var typeFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], prefabFile);
            var filter = Builders<GridFSFileInfo>.Filter.And(projectIdFilter, typeFilter);

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var options = new GridFSFindOptions
            {
                Limit = 1,
                Sort = sort
            };
            using (var cursor = await bucket.FindAsync(filter, options))
            {
                var fileInfo = (await cursor.ToListAsync()).FirstOrDefault();
                return await bucket.DownloadAsBytesAsync(fileInfo.Id);
            }
        }

        ///<inheritdoc/>
        public async Task<byte[]> GetPrefabDataFilesAsync(string projectId, string version)
        {
            Guard.Argument(projectId).NotNull().NotEmpty();
            Guard.Argument(version).NotNull().NotEmpty();
            var projectIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["prefabId"], projectId);
            var projectVersionFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["version"], version);
            var typeFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], prefabDataFiles);
            var filter = Builders<GridFSFileInfo>.Filter.And(projectIdFilter, projectVersionFilter, typeFilter);

            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var options = new GridFSFindOptions
            {
                Limit = 1,
                Sort = sort
            };
            using (var cursor = await bucket.FindAsync(filter, options))
            {
                var fileInfo = (await cursor.ToListAsync()).FirstOrDefault();
                return await bucket.DownloadAsBytesAsync(fileInfo.Id);
            }
        }


        ///<inheritdoc/>
        public async IAsyncEnumerable<PrefabMetaDataCompact> GetPrefabsMetadataForApplicationAsync(string applicationId)
        {
            var typeFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], "PrefabFile");
            var applicationFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["applicationId"], applicationId);
            var filter = Builders<GridFSFileInfo>.Filter.And(typeFilter, applicationFilter);

            using (var cursor = await bucket.FindAsync(filter, new GridFSFindOptions()))
            {
                var files = await cursor.ToListAsync();

                foreach (var group in files.GroupBy(a => a.Metadata["prefabId"]))
                {
                    var file = group.OrderByDescending(a => a.UploadDateTime).FirstOrDefault();
                    var prefabMetaData = await GetPrefabMetadataForPrefabAsync(applicationId, file.Metadata["prefabId"].AsString);
                    prefabMetaData.LastUpdated = file.UploadDateTime;
                    yield return prefabMetaData;
                }
                yield break;
            }
        }

        ///<inheritdoc/>
        public async Task<PrefabMetaDataCompact> GetPrefabMetadataForPrefabAsync(string applicationId, string prefabId)
        {
            var projectIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["prefabId"], prefabId);
            var typeFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], "PrefabDataFiles");
            var filter = Builders<GridFSFileInfo>.Filter.And(projectIdFilter, typeFilter);

            var prefabMetaData = new PrefabMetaDataCompact() { PrefabId = prefabId, ApplicationId = applicationId };
            using (var cursor = await bucket.FindAsync(filter, new GridFSFindOptions()))
            {
                var files = await cursor.ToListAsync();
                foreach (var group in files.GroupBy(a => a.Metadata["version"]))
                {
                    var file = group.OrderByDescending(a => a.UploadDateTime).FirstOrDefault();
                    prefabMetaData.AddOrUpdateVersionMetaData(new PrefabVersionMetaData()
                    {
                        Version = file.Metadata["version"].AsString,
                        IsActive = file.Metadata["isActive"].AsBoolean,
                        IsDeployed = file.Metadata["isDeployed"].AsBoolean,
                        LastUpdated = file.UploadDateTime
                    });
                }
            }
            return prefabMetaData;
        }


        ///<inheritdoc/>
        public async Task PurgeRevisionFiles(RetentionPolicy purgeStrategy)
        {
            Guard.Argument(purgeStrategy).NotNull();

            await PurgeProjectFiles();
            await PurgeProjectDataFiles(purgeStrategy);
        }

        /// <summary>
        /// Delete the revisions for project file. We want to keep only the latest for the project file
        /// and previous revisios can be deleted.
        /// </summary>
        /// <returns></returns>
        private async Task PurgeProjectFiles()
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], prefabFile);
            using (var cursor = await bucket.FindAsync(filter))
            {
                var files = await cursor.ToListAsync();

                foreach (var group in files.GroupBy(a => a.Metadata["prefabId"]))
                {
                    var fileRevisions = group.OrderByDescending(a => a.UploadDateTime);
                    foreach (var fileRevision in fileRevisions.Skip(1))
                    {
                        logger.LogInformation("Deleting project file with Id : {Id} for prefab : {prefab}",
                            fileRevision.Id, fileRevision.Metadata["prefabId"]);
                        await bucket.DeleteAsync(fileRevision.Id);
                    }

                }
            }
        }

        /// <summary>
        /// Delete the revisions for the project data file as per specified retention policy.
        /// </summary>
        /// <param name="policy"></param>
        /// <returns></returns>
        private async Task PurgeProjectDataFiles(RetentionPolicy policy)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], prefabDataFiles);
            using (var cursor = await bucket.FindAsync(filter))
            {
                var files = await cursor.ToListAsync();
                foreach (var group in files.GroupBy(a => a.Metadata["prefabId"]))
                {
                    var fileRevisions = group.OrderByDescending(a => a.UploadDateTime);
                    //Delete all the revisions that exceed max allowed number of revisions
                    foreach (var fileRevision in fileRevisions.Skip(policy.MaxNumberOfRevisions))
                    {
                        //don't delete the deployed version as there will be a single
                        //entry per version with isDeployed = true
                        if (fileRevision.Metadata["isDeployed"].AsBoolean)
                        {
                            continue;
                        }
                        logger.LogInformation("Deleting project data file with Id : {Id} for project : {projectId},  version {version} updated on {updated}",
                            fileRevision.Id, fileRevision.Metadata["prefabId"], fileRevision.Metadata["version"],
                            fileRevision.UploadDateTime.ToUniversalTime().ToString());
                        await bucket.DeleteAsync(fileRevision.Id);
                    }

                    //Delete all the revisions that are older then the allowed age
                    foreach (var fileRevision in fileRevisions.Skip(1))
                    {
                        //don't delete the deployed version as there will be a single
                        //entry per version with isDeployed = true
                        if (fileRevision.Metadata["isDeployed"].AsBoolean)
                        {
                            continue;
                        }
                        if (DateTime.Now.ToUniversalTime().Subtract(fileRevision.UploadDateTime) > TimeSpan.FromDays(policy.MaxAgeOfRevisions))
                        {
                            logger.LogInformation("Deleting project data file with Id : {Id} for prefab : {prefabId},  version {version} updated on {updated}",
                            fileRevision.Id, fileRevision.Metadata["prefabId"], fileRevision.Metadata["version"],
                            fileRevision.UploadDateTime.ToUniversalTime().ToString());
                            await bucket.DeleteAsync(fileRevision.Id);
                        }
                    }
                }
            }
        }
    }
}
