using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository;

/// <summary>
/// Default implementation of <see cref="IComposeFilesRepository"/>
/// </summary>
public class ComposeFilesRepository : IComposeFilesRepository
{
    private readonly ILogger logger;
    private readonly IGridFSBucket bucket;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="dbSettings"></param>
    public ComposeFilesRepository(ILogger<ComposeFilesRepository> logger, IMongoDbSettings dbSettings)
    {
        this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
        var client = new MongoClient(dbSettings.ConnectionString);
        var database = client.GetDatabase(dbSettings.DatabaseName);
        this.bucket = new GridFSBucket(database, new GridFSBucketOptions
        {
            BucketName = dbSettings.ComposeFilesBucketName
        });
    }

    /// <inheritdoc/>   
    public async Task<bool> CheckFileExistsAsync(string fileName)
    {
        Guard.Argument(fileName, nameof(fileName)).NotNull().NotEmpty();

        var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, fileName);
        var options = new GridFSFindOptions();

        using (var cursor = await bucket.FindAsync(filter, options))
        {
            return cursor.Any();
        }
    }  

    /// <inheritdoc/>   
    public async Task<DataFile> GetFileAsync(string fileName)
    {
        Guard.Argument(fileName, nameof(fileName)).NotNull().NotEmpty();

        var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, fileName);
        var options = new GridFSFindOptions();

        using (var cursor = await bucket.FindAsync(filter, options))
        {
            var file = await cursor.FirstOrDefaultAsync() ?? throw new GridFSFileNotFoundException($"File {fileName} doesn't exist");
            return new DataFile()
            {
                FileName = fileName,
                Bytes = await bucket.DownloadAsBytesAsync(file.Id)
            };
        }
    }

    /// <inheritdoc/>   
    public async IAsyncEnumerable<string> GetAllFileNamesAsync()
    {
        var filter = Builders<GridFSFileInfo>.Filter.Empty;
        var options = new GridFSFindOptions();
        using (var cursor = await bucket.FindAsync(filter, options))
        {
            foreach (var file in (await cursor.ToListAsync()))
            {
                yield return file.Filename;
            }
        }
    }

    /// <inheritdoc/>   
    public async IAsyncEnumerable<DataFile> GetAllFilesAsync()
    {
        var filter = Builders<GridFSFileInfo>.Filter.Empty;
        var options = new GridFSFindOptions();
        using (var cursor = await bucket.FindAsync(filter, options))
        {
            foreach (var file in (await cursor.ToListAsync()))
            {
                var fileData = await bucket.DownloadAsBytesAsync(file.Id);
                yield return new DataFile()
                {
                    FileName = file.Filename,
                    Bytes = fileData
                };
            }
        }
    }

    /// <inheritdoc/>   
    public async Task AddOrUpdateFileAsync(string fileName, byte[] fileContents)
    {
        var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, fileName);

        var file = (await bucket.FindAsync(filter)).FirstOrDefault();
        if (file != null)
        {
            await bucket.DeleteAsync(file.Id);
            logger.LogInformation("Deleted existing copy of file : {0} ", fileName);
        }
;
        await bucket.UploadFromBytesAsync(fileName, fileContents, new GridFSUploadOptions());
        logger.LogInformation("File {0} was added.", fileName);
    }

    /// <inheritdoc/>   
    public async Task DeleteFileAsync(string fileName)
    {
        var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, fileName);

        using (var cursor = await bucket.FindAsync(filter, new GridFSFindOptions()))
        {
            var files = await cursor.ToListAsync();
            foreach (var file in files)
            {
                await bucket.DeleteAsync(file.Id);
                logger.LogInformation("File {0} was deleted.", fileName);
            }
        }
    }
}
