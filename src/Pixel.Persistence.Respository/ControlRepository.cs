using Dawn;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class ControlRepository : IControlRepository
    {
        private readonly IGridFSBucket controlBucket;
        private readonly IGridFSBucket imageBucket;

        public ControlRepository(IMongoDbSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            controlBucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = dbSettings.ControlsBucketName
            });
            imageBucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = dbSettings.ImagesBucketName
            });

        }

        public async Task AddOrUpdateControl(ControlMetaData control, string fileName, byte[] fileData)
        {
            Guard.Argument(control).NotNull();
            await controlBucket.UploadFromBytesAsync(fileName, fileData, new GridFSUploadOptions()
            {
                Metadata = new MongoDB.Bson.BsonDocument()
                {
                    {"applicationId" , control.ApplicationId },
                    {"controlId" , control.ControlId },
                    {"controlName", control.ControlName }
                }
            });           
        }

        public async Task AddOrUpdateControlImage(ControlImageMetaData control, string fileName, byte[] fileData)
        {
            Guard.Argument(control).NotNull();        
            await imageBucket.UploadFromBytesAsync(fileName, fileData, new GridFSUploadOptions()
            {
                Metadata = new MongoDB.Bson.BsonDocument()
                {
                    {"applicationId" , control.ApplicationId},
                    {"controlId" , control.ControlId },
                    {"resolution", control.Resolution }
                }
            });
        }

        public async IAsyncEnumerable<DataFile> GetControlFiles(string applicationId, string controlId)
        {
            Guard.Argument(applicationId).NotNull().NotEmpty();
            Guard.Argument(controlId).NotNull().NotEmpty();
            var applicationIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["applicationId"], applicationId);
            var controlIdFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["controlId"], controlId);
            var filter = Builders<GridFSFileInfo>.Filter.And(applicationIdFilter, controlIdFilter);
            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var options = new GridFSFindOptions
            {
                Limit = 1,
                Sort = sort
            };
            using (var cursor = await controlBucket.FindAsync(filter, options))
            {
                var fileInfo = (await cursor.ToListAsync()).FirstOrDefault();
                var result = await controlBucket.DownloadAsBytesAsync(fileInfo.Id);
                yield return new DataFile() { FileName = fileInfo.Filename, Bytes = result, Type = "ControlFile" };
            }
            //Get all the images
            using (var cursor = await imageBucket.FindAsync(filter, new GridFSFindOptions()))
            {
                var imageFiles = await cursor.ToListAsync();
                foreach(var imageFile in imageFiles)
                {
                    var result = await imageBucket.DownloadAsBytesAsync(imageFile.Id);
                    yield return new ImageDataFile() { FileName = imageFile.Filename, Bytes = result, Resolution = imageFile.Metadata["resolution"].AsString, Type = "ControlImage" };
                }
            }
           
        }   

        public async IAsyncEnumerable<ControlMetaData> GetMetadataAsync(string applicationId)
        {

            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["applicationId"], applicationId);
            //var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            //var options = new GridFSFindOptions
            //{
            //    Limit = 1,
            //    Sort = sort
            //};
            using (var cursor = await controlBucket.FindAsync(filter, new GridFSFindOptions()))
            {
                var files = await cursor.ToListAsync();
                foreach (var group in files.GroupBy(a => a.Metadata["controlId"]))
                {
                    var file = group.OrderByDescending(a => a.UploadDateTime).FirstOrDefault();
                    yield return new ControlMetaData()
                    {                       
                        ControlId = file.Metadata["controlId"].AsString,
                        LastUpdated = file.UploadDateTime
                    };

                }
                yield break;
            }
        }
    }
}
