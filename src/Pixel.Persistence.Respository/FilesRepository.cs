using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository;

public class ProjectFilesRepository : FilesRepository , IProjectFilesRepository
{
    public ProjectFilesRepository(ILogger<FilesRepository> logger, IMongoDbSettings dbSettings)
        :base(logger, dbSettings)
    {             
    }

    protected override IGridFSBucket GetBucket(IMongoDbSettings dbSettings)
    {
        var client = new MongoClient(dbSettings.ConnectionString);
        var database = client.GetDatabase(dbSettings.DatabaseName);
        return new GridFSBucket(database, new GridFSBucketOptions
        {
            BucketName = dbSettings.ProjectFilesBucketName
        });
    }

    protected override (string, string) GetIdAndVersionFieldNames()
    {
        return ("projectId", "projectVersion");
    }
}

public class PrefabFilesRepository : FilesRepository, IPrefabFilesRepository
{
    public PrefabFilesRepository(ILogger<FilesRepository> logger, IMongoDbSettings dbSettings)
        : base(logger, dbSettings)
    {

    }

    protected override IGridFSBucket GetBucket(IMongoDbSettings dbSettings)
    {
        var client = new MongoClient(dbSettings.ConnectionString);
        var database = client.GetDatabase(dbSettings.DatabaseName);
        return new GridFSBucket(database, new GridFSBucketOptions
        {
            BucketName = dbSettings.PrefabFilesBucketName
        });
    }

    protected override (string, string) GetIdAndVersionFieldNames()
    {
        return ("prefabId", "prefabVersion");
    }
}

public abstract class FilesRepository : IFilesRepository
{
    protected readonly ILogger logger;
    protected readonly IGridFSBucket bucket;
    protected readonly string IdField;
    protected readonly string versionField;
    protected readonly string filePathField;
    protected readonly string tagField;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="dbSettings"></param>
    public FilesRepository(ILogger<FilesRepository> logger, IMongoDbSettings dbSettings)
    {
        Guard.Argument(dbSettings).NotNull();
        this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
        (this.IdField, this.versionField) = GetIdAndVersionFieldNames();
        this.filePathField = "filePath";
        this.tagField = "tag";
        this.bucket = GetBucket(dbSettings);
    }

    protected abstract IGridFSBucket GetBucket(IMongoDbSettings dbSettings);

    protected abstract (string, string) GetIdAndVersionFieldNames();

    /// <inheritdoc/>   
    public async Task<ProjectDataFile> GetFileAsync(string projectId, string projectVersion, string fileName)
    {
        var projectIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata[this.IdField], projectId);
        var projectVersionFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata[this.versionField], projectVersion);
        var fileNameFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, fileName);
        var filter = Builders<GridFSFileInfo>.Filter.And(projectIdFilter, projectVersionFilter, fileNameFilter);

        var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
        var options = new GridFSFindOptions
        {
            Limit = 1,
            Sort = sort
        };

        using (var cursor = await bucket.FindAsync(filter, options))
        {
            var fileInfo = (await cursor.ToListAsync()).FirstOrDefault();
            if(fileInfo != null)
            {
                var fileData = await bucket.DownloadAsBytesAsync(fileInfo.Id);
                return new ProjectDataFile()
                {
                    ProjectId = projectId,
                    ProjectVersion = projectVersion,
                    FileName = fileInfo.Filename,
                    FilePath = fileInfo.Metadata[this.filePathField].AsString,
                    Bytes = fileData
                };
            }          
        }
        return null;
    }

    /// <inheritdoc/>   
    public async IAsyncEnumerable<ProjectDataFile> GetFilesAsync(string projectId, string projectVersion, string[] tags)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(tags, nameof(tags)).NotNull();

        var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata[this.IdField], projectId) &
                     Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata[this.versionField], projectVersion);
                     
        if(tags.Any())
        {
            filter = filter &  Builders<GridFSFileInfo>.Filter.In(x => x.Metadata[this.tagField], tags.Select(s => BsonValue.Create(s)));
        }        

        var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
        var options = new GridFSFindOptions
        {
            Sort = sort
        };

        using (var cursor = await bucket.FindAsync(filter, options))
        {
            foreach (var file in (await cursor.ToListAsync()))
            {
                var fileData = await bucket.DownloadAsBytesAsync(file.Id);
                yield return new ProjectDataFile()
                {
                    ProjectId = projectId,
                    ProjectVersion = projectVersion,
                    FileName = file.Filename,
                    FilePath = file.Metadata[this.filePathField].AsString,
                    Tag = file.Metadata[this.tagField].AsString,
                    Bytes = fileData
                };
            }
        }
    }

    /// <inheritdoc/>   
    public async IAsyncEnumerable<ProjectDataFile> GetFilesOfTypeAsync(string projectId, string projectVersion, string fileExtension)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(fileExtension, nameof(fileExtension)).NotNull().NotEmpty();

        var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata[this.IdField], projectId) &
                     Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata[this.versionField], projectVersion) &
                     Builders<GridFSFileInfo>.Filter.Regex(x => x.Filename, $"({fileExtension})$");     

        var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
        var options = new GridFSFindOptions
        {
            Sort = sort
        };

        using (var cursor = await bucket.FindAsync(filter, options))
        {
            foreach (var file in (await cursor.ToListAsync()))
            {
                var fileData = await bucket.DownloadAsBytesAsync(file.Id);
                yield return new ProjectDataFile()
                {
                    ProjectId = projectId,
                    ProjectVersion = projectVersion,
                    FileName = file.Filename,
                    FilePath = file.Metadata[this.filePathField].AsString,
                    Tag = file.Metadata[this.tagField].AsString,
                    Bytes = fileData
                };
            }
        }
    }

    /// <inheritdoc/>   
    public async Task AddOrUpdateFileAsync(string projectId, string projectVersion, ProjectDataFile dataFile)
    {
        var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata[this.IdField], projectId) &
                     Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata[this.versionField], projectVersion) &
                     Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, dataFile.FileName);

        var file = (await bucket.FindAsync(filter)).FirstOrDefault();
        if(file != null)
        {
            await bucket.DeleteAsync(file.Id);
            logger.LogInformation("Deleted existing version of file {0} from project {1}", dataFile.FileName, dataFile.ProjectId);
        }

        dataFile.ProjectId = projectId;
        dataFile.ProjectVersion = projectVersion;
        await bucket.UploadFromBytesAsync(dataFile.FileName, dataFile.Bytes, new GridFSUploadOptions()
        {
            Metadata = new BsonDocument()
                        {
                            {this.IdField , dataFile.ProjectId},
                            {this.versionField, dataFile.ProjectVersion},
                            {this.filePathField, dataFile.FilePath },
                            {this.tagField, dataFile.Tag }
                        }
        });
        logger.LogInformation("File {0} was added to project {1}", dataFile.FileName, dataFile.ProjectId);
    }

    /// <inheritdoc/>   
    public async Task DeleteFileAsync(string projectId, string projectVersion, string fileName)
    {      
        var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata[this.IdField], projectId) &
                     Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata[this.versionField], projectVersion) &
                     Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, fileName);

        using (var cursor = await bucket.FindAsync(filter, new GridFSFindOptions()))
        {
            var files = await cursor.ToListAsync();
            foreach (var file in files)
            {
                await bucket.DeleteAsync(file.Id);
            }
        }
    }
}
