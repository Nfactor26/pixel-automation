using Dawn;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{    
    public class ControlRepository : IControlRepository
    {
        private readonly IMongoCollection<BsonDocument> controlsCollection;    
        private readonly IGridFSBucket imageBucket;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dbSettings"></param>
        public ControlRepository(IMongoDbSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            controlsCollection = database.GetCollection<BsonDocument>(dbSettings.ControlsCollectionName);
            imageBucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = dbSettings.ImagesBucketName
            });

        }

        ///<inheritdoc/>
        public async Task AddOrUpdateControl(string controlDataJson)
        {
            Guard.Argument(controlDataJson, nameof(controlDataJson)).NotNull().NotEmpty();
       
            if (BsonDocument.TryParse(controlDataJson, out BsonDocument document))
            {
                string applicationId = document["ApplicationId"].AsString;
                string controlId = document["ControlId"].AsString;

                document.Add("LastUpdated", DateTime.Now.ToUniversalTime());            
              
                //if document with application id already exists, replace it
                if (controlsCollection.CountDocuments<BsonDocument>(x => x["ApplicationId"].Equals(applicationId) 
                    && x["ControlId"].Equals(controlId), new CountOptions { Limit = 1 }) > 0)
                {
                    await controlsCollection.FindOneAndReplaceAsync<BsonDocument>(CreateControlFilter(applicationId, controlId), document);
                }
                else
                {
                    await controlsCollection.InsertOneAsync(document);
                }
                return;
            }

            throw new ArgumentException("Failed to parse control data in to BsonDocument");
        }

        ///<inheritdoc/>
        public async Task AddOrUpdateControlImage(ControlImageMetaData imageMetaData, byte[] fileData)
        {
            Guard.Argument(imageMetaData, nameof(imageMetaData)).NotNull();
           
            //Delete any existing version of image file. We don't want revisions of image.
            await DeleteImageAsync(imageMetaData);
          
            await imageBucket.UploadFromBytesAsync(imageMetaData.FileName, fileData, new GridFSUploadOptions()
            {
                Metadata = new BsonDocument()
                {
                    {"applicationId" , imageMetaData.ApplicationId},
                    {"controlId" , imageMetaData.ControlId }                   
                }
            });
          
        }

        ///<inheritdoc/>
        public async Task DeleteImageAsync(ControlImageMetaData imageMetaData)
        {
            Guard.Argument(imageMetaData, nameof(imageMetaData)).NotNull();
            var filter = CreateImageFilter(imageMetaData.ApplicationId, imageMetaData.ControlId);
            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var options = new GridFSFindOptions
            {
                Limit = 1,
                Sort = sort
            };
         
            using (var cursor = await imageBucket.FindAsync(filter, new GridFSFindOptions()))
            {
                var imageFiles = await cursor.ToListAsync();
                foreach (var imageFile in imageFiles)
                {
                    await imageBucket.DeleteAsync(imageFile.Id);
                }
            }
        }

        ///<inheritdoc/>
        public async IAsyncEnumerable<DataFile> GetControlFiles(string applicationId, string controlId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotEmpty();
            Guard.Argument(controlId, nameof(controlId)).NotNull().NotEmpty();

            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("LastUpdated");
            var result = controlsCollection.Find<BsonDocument>(CreateControlFilter(applicationId, controlId)).Project(projection);
            var document = await result.SingleOrDefaultAsync();          
            using(MemoryStream ms = new MemoryStream())
            {
                using(StreamWriter sw = new StreamWriter(ms, System.Text.Encoding.UTF8))
                {
                    var jsonData = JsonSerializer.Serialize(BsonTypeMapper.MapToDotNetValue(document), new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    }) ;
                    sw.Write(jsonData);
                    sw.Flush();
                    yield return new DataFile() { FileName = $"{controlId}.dat", Bytes = ms.ToArray(), Type = "ControlFile" };

                }
            }

            var filter = CreateImageFilter(applicationId, controlId);
            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var options = new GridFSFindOptions
            {
                Limit = 1,
                Sort = sort
            };
            //Get all the images
            using (var cursor = await imageBucket.FindAsync(filter, new GridFSFindOptions()))
            {
                var imageFiles = await cursor.ToListAsync();
                foreach(var imageFile in imageFiles)
                {
                    var imageBytes = await imageBucket.DownloadAsBytesAsync(imageFile.Id);
                    yield return new DataFile() { FileName = imageFile.Filename, Bytes = imageBytes, Type = "ControlImage" };
                }
            }
           
        }

        ///<inheritdoc/>
        public async IAsyncEnumerable<ControlMetaData> GetMetadataAsync(string applicationId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotEmpty();

            var filter = Builders<BsonDocument>.Filter.Empty;
            var projection = Builders<BsonDocument>.Projection.Include("ControlId").Include("LastUpdated");
            var results = await controlsCollection.Find<BsonDocument>(filter).Project(projection).ToListAsync();
            foreach (var doc in results)
            {
                yield return new ControlMetaData()
                {
                    ControlId = doc["ControlId"].AsString,
                    LastUpdated = doc["LastUpdated"].ToUniversalTime()
                };
            }
        }

        /// <summary>
        /// Create filter condition for control file with given applicationId and controlId
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        private FilterDefinition<BsonDocument> CreateControlFilter(string applicationId, string controlId)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq(x => x["ApplicationId"], applicationId);
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x["ControlId"], controlId));
            return filter;
        }
     
        /// <summary>
        /// Create filter condition for image with given applicationId and controlId.        
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        private FilterDefinition<GridFSFileInfo> CreateImageFilter(string applicationId, string controlId)
        {
            var filterBuilder = Builders<GridFSFileInfo>.Filter;
            var filter = filterBuilder.Eq(x => x.Metadata["applicationId"], applicationId);
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.Metadata["controlId"], controlId));
            return filter;
        }
    }
}
