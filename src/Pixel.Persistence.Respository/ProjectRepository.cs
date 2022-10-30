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
    public class ProjectRepository : IProjectRepository
    {
        private readonly string projectFile = "ProjectFile";
        private readonly string projectDataFiles = "ProjectDataFiles";
      
        private readonly ILogger logger;
        private readonly IGridFSBucket bucket;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dbSettings"></param>
        public ProjectRepository(ILogger<ProjectRepository> logger, IMongoDbSettings dbSettings)
        {
            Guard.Argument(dbSettings).NotNull();
            this.logger = Guard.Argument(logger).NotNull().Value;

            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            bucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = dbSettings.ProjectsBucketName
            });
        }

        ///<inheritdoc/>
        public async Task AddOrUpdateProject(ProjectMetaData project, string fileName, byte[] fileData)
        {
            Guard.Argument(project).NotNull();
            
            switch (project.Type)
            {
                case "ProjectFile":              
                    await bucket.UploadFromBytesAsync(fileName, fileData, new GridFSUploadOptions()
                    {
                        Metadata = new MongoDB.Bson.BsonDocument()
                         {
                            {"projectId" , project.ProjectId},
                            {"type", project.Type}
                         }
                    });
                    break;

                case "ProjectDataFiles":
                    var projectIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["projectId"], project.ProjectId);                  
                    var projectVersionFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["version"], project.Version);
                    var filter = Builders<GridFSFileInfo>.Filter.And(projectIdFilter, projectVersionFilter);
                    var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
                    var options = new GridFSFindOptions
                    {
                        Limit = 1,
                        Sort = sort
                    };
                    using (var cursor = await bucket.FindAsync(filter, options))
                    {
                        var fileInfo = (await cursor.ToListAsync()).FirstOrDefault();
                        if (!(fileInfo?.Metadata["isActive"].AsBoolean ?? true))
                        {
                            throw new InvalidOperationException($"Version : {project.Version} for project with id : {project.ProjectId} is not active. Can't update a published version");
                        }
                    }

                    await bucket.UploadFromBytesAsync(fileName, fileData, new GridFSUploadOptions()
                    {
                        Metadata = new MongoDB.Bson.BsonDocument()
                        {
                            {"projectId" , project.ProjectId},
                            {"version", project.Version},
                            {"type", project.Type},
                            {"isActive", project.IsActive}
                        }
                    });
                    break;

                default:
                    throw new ArgumentException($"type : {project.Type} is not supported. Valid values are ProjectFile for (.atm) file and ProjectDataFiles for contents of a project version (process, test cases, scripts, etc)");
            }

        }

        ///<inheritdoc/>
        public async Task<byte[]> GetProjectFile(string projectId)
        {
            Guard.Argument(projectId).NotNull().NotEmpty();           
            var projectIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["projectId"], projectId);
            var typeFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], "ProjectFile");
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
        public async Task<byte[]> GetProjectDataFiles(string projectId, string version)
        {
            Guard.Argument(projectId).NotNull().NotEmpty();
            Guard.Argument(version).NotNull().NotEmpty();
            var projectIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["projectId"], projectId);
            var projectVersionFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["version"], version);
            var typeFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], projectDataFiles);
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
        public async IAsyncEnumerable<ProjectMetaData> GetProjectsMetadataAsync()
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], projectFile);

            using (var cursor = await bucket.FindAsync(filter, new GridFSFindOptions()))
            {
                var files = await cursor.ToListAsync();

                foreach (var group in files.GroupBy(a => a.Metadata["projectId"]))
                {
                    var file = group.OrderByDescending(a => a.UploadDateTime).FirstOrDefault();
                    yield return new ProjectMetaData()
                    {
                        ProjectId = file.Metadata["projectId"].AsString,
                        LastUpdated = file.UploadDateTime
                    };
                }
                yield break;
            }
        }
      
        ///<inheritdoc/>
        public async IAsyncEnumerable<ProjectMetaData> GetProjectMetadataAsync(string projectId)
        {
            var projectIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["projectId"], projectId);
            var typeFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], projectDataFiles);
            var filter = Builders<GridFSFileInfo>.Filter.And(projectIdFilter, typeFilter);

            using (var cursor = await bucket.FindAsync(filter, new GridFSFindOptions()))
            {
                var files = await cursor.ToListAsync();
                foreach (var group in files.GroupBy(a => a.Metadata["version"]))
                {
                    var file = group.OrderByDescending(a => a.UploadDateTime).FirstOrDefault();
                    yield return new ProjectMetaData()
                    {
                        ProjectId = file.Metadata["projectId"].AsString,
                        Version = file.Metadata["version"].AsString,
                        Type = file.Metadata["type"].AsString,
                        IsActive = file.Metadata["isActive"].AsBoolean,
                        LastUpdated = file.UploadDateTime
                    };

                }
                yield break;
            }
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
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], projectFile);
            using (var cursor = await bucket.FindAsync(filter))
            {
                var files = await cursor.ToListAsync();

                foreach (var group in files.GroupBy(a => a.Metadata["projectId"]))
                {
                    var fileRevisions = group.OrderByDescending(a => a.UploadDateTime);
                    foreach (var fileRevision in fileRevisions.Skip(1))
                    {
                        logger.LogInformation("Deleting project file with Id : {Id} for project : {projectId}",
                            fileRevision.Id, fileRevision.Metadata["projectId"]);
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
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], projectDataFiles);            
            using (var cursor = await bucket.FindAsync(filter))
            {
                var files = await cursor.ToListAsync();
                foreach (var group in files.GroupBy(a => a.Metadata["projectId"]))
                {
                    var fileRevisions = group.OrderByDescending(a => a.UploadDateTime);
                    //Delete all the revisions that exceed max allowed number of revisions
                    foreach (var fileRevision in fileRevisions.Skip(policy.MaxNumberOfRevisions))
                    {
                        //don't delete the published version as there will be a single entry per version with isActive = false
                        if(!fileRevision.Metadata["isActive"].AsBoolean)
                        {
                            continue;
                        }
                        logger.LogInformation("Deleting project data file with Id : {Id} for project : {projectId},  version {version} updated on {updated}",
                            fileRevision.Id, fileRevision.Metadata["projectId"], fileRevision.Metadata["version"], 
                            fileRevision.UploadDateTime.ToUniversalTime().ToString());
                        await bucket.DeleteAsync(fileRevision.Id);
                    }

                    //Delete all the revisions that are older then the allowed age
                    foreach (var fileRevision in fileRevisions.Skip(1))
                    {
                        //don't delete the published version as there will be a single entry per version with isActive = false
                        if (!fileRevision.Metadata["isActive"].AsBoolean)
                        {
                            continue;
                        }                        
                        if(DateTime.Now.ToUniversalTime().Subtract(fileRevision.UploadDateTime) > TimeSpan.FromDays(policy.MaxAgeOfRevisions))
                        {
                            logger.LogInformation("Deleting project data file with Id : {Id} for project : {projectId},  version {version} updated on {updated}",
                            fileRevision.Id, fileRevision.Metadata["projectId"], fileRevision.Metadata["version"],
                            fileRevision.UploadDateTime.ToUniversalTime().ToString());
                            await bucket.DeleteAsync(fileRevision.Id);
                        }
                    }
                }                
            }
        }
    }
}
