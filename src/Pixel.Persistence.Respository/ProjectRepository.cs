using Dawn;
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
        private readonly IGridFSBucket bucket;

        public ProjectRepository(IMongoDbSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            bucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = dbSettings.ProjectsBucketName
            });
        }


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
                        if (fileInfo?.Metadata["isDeployed"].AsBoolean ?? false)
                        {
                            throw new InvalidOperationException($"Version : {project.Version} for project with id : {project.ProjectId} is deployed. Can't update a deployed version");
                        }
                    }

                    await bucket.UploadFromBytesAsync(fileName, fileData, new GridFSUploadOptions()
                    {
                        Metadata = new MongoDB.Bson.BsonDocument()
                    {
                        {"projectId" , project.ProjectId},
                        {"version", project.Version},
                        {"type", project.Type},
                        {"isActive", project.IsActive},
                        {"isDeployed", project.IsDeployed}
                    }
                    });
                    break;
                default:
                    throw new ArgumentException($"type : {project.Type} is not supported. Valid values are ProjectFile for (.atm) file and ProjectDataFiles for contents of a project version (process, test cases, scripts, etc)");
            }

        }


        public async IAsyncEnumerable<ProjectMetaData> GetProjectsMetadataAsync()
        {           
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], "ProjectFile");         

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


        public async IAsyncEnumerable<ProjectMetaData> GetProjectMetadataAsync(string projectId)
        {
            var projectIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["projectId"], projectId);
            var typeFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], "ProjectDataFiles");
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
                        IsDeployed = file.Metadata["isDeployed"].AsBoolean,
                        LastUpdated = file.UploadDateTime
                    };

                }
                yield break;
            }
        }

        /// <summary>
        /// Get the project file (.atm) for a given projectId.
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Get the version content file (zipped) for a project which includes process, test cases, scripts, assembiles, etc. for that version
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public async Task<byte[]> GetProjectDataFiles(string projectId, string version)
        {
            Guard.Argument(projectId).NotNull().NotEmpty();
            Guard.Argument(version).NotNull().NotEmpty();
            var projectIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["projectId"], projectId);
            var projectVersionFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["version"], version);
            var typeFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["type"], "ProjectDataFiles");
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
    }
}
