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
    public class ApplicationRepository : IApplicationRepository
    {       
        private readonly IGridFSBucket bucket;

        public ApplicationRepository(IMongoDbSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);           
            bucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = dbSettings.ApplicationsBucketName
            });

        }

        public async Task<byte[]> GetApplicationFile(string applicationId)
        {
            Guard.Argument(applicationId).NotNull().NotEmpty();
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["applicationId"], applicationId);
            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var options = new GridFSFindOptions
            {
                Limit = 1,
                Sort = sort
            };
            using (var cursor = await bucket.FindAsync(filter, options))
            {
                var fileInfo = (await cursor.ToListAsync()).FirstOrDefault();
                return await bucket.DownloadAsBytesByNameAsync(fileInfo.Filename);
            }                
        }

        public async IAsyncEnumerable<ApplicationMetaData> GetMetadataAsync()
        {
            
            var filter = Builders<GridFSFileInfo>.Filter.Empty;
            //var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            //var options = new GridFSFindOptions
            //{               
            //    Sort = sort
            //};
            using (var cursor = await bucket.FindAsync(filter, new GridFSFindOptions()))
            {
                var files = await cursor.ToListAsync();              
                foreach(var group in files.GroupBy(a => a.Metadata["applicationId"]))
                {
                    var file = group.OrderByDescending(a => a.UploadDateTime).FirstOrDefault();
                    yield return new ApplicationMetaData() 
                    { 
                        ApplicationId = file.Metadata["applicationId"].AsString,
                        LastUpdated = file.UploadDateTime
                    };
                 
                }
                yield break;
            }
        }

        public async Task AddOrUpdate(ApplicationMetaData application, string fileName,  byte[] fileData)
        {
            Guard.Argument(application).NotNull();        
            await bucket.UploadFromBytesAsync(fileName, fileData, new GridFSUploadOptions()
            {
                Metadata = new MongoDB.Bson.BsonDocument()
                {
                    {"applicationId" , application.ApplicationId },
                    {"applicationName", application.ApplicationName},
                    {"applicationType", application.ApplicationType}
                }
            });
        }
    }
}
