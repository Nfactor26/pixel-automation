using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class ControlRepository : IControlRepository
    {
        private readonly ILogger logger;
        private readonly IMongoCollection<BsonDocument> controlsCollection;
        private readonly IMongoCollection<ProjectReferences> referencesCollection;
        private readonly IGridFSBucket imageBucket;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dbSettings"></param>
        public ControlRepository(ILogger<ControlRepository> logger, IMongoDbSettings dbSettings)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            controlsCollection = database.GetCollection<BsonDocument>(dbSettings.ControlsCollectionName);
            referencesCollection = database.GetCollection<ProjectReferences>(dbSettings.ProjectReferencesCollectionName);
            imageBucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = dbSettings.ImagesBucketName
            });

        }

        async Task<bool> CheckControlExists(string applicationId, string controlId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            Guard.Argument(controlId, nameof(controlId)).NotNull().NotWhiteSpace();
            var filter = CreateControlFilter(applicationId, controlId);
            var count = await controlsCollection.CountDocumentsAsync(filter);
            return count > 0;
        }

        async Task<bool> CheckControlVersionExists(string applicationId, string controlId, string controlVersion)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            Guard.Argument(controlId, nameof(controlId)).NotNull().NotWhiteSpace();
            var filter = CreateControlFilter(applicationId, controlId, controlVersion);
            var count = await controlsCollection.CountDocumentsAsync(filter);
            return count > 0;
        }


        async Task<BsonDocument> FindControlByIdAsync(string applicationId, string controlId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            var filter = CreateControlFilter(applicationId, controlId);
            //exclude field not known to client.
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("LastUpdated").Exclude("ApplicationId").Exclude("Revision");
            var result = controlsCollection.Find<BsonDocument>(filter).Project(projection);
            var document = await result.SingleOrDefaultAsync();
            return document;
        }

        ///<inheritdoc/>
        public async IAsyncEnumerable<DataFile> GetControlFiles(string applicationId, string controlId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotEmpty();
            Guard.Argument(controlId, nameof(controlId)).NotNull().NotEmpty();

            //Get all the versoins of the control
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("LastUpdated");
            var result = controlsCollection.Find<BsonDocument>(CreateControlFilter(applicationId, controlId)).Project(projection);
            var documents = await result.ToListAsync();
            foreach (var document in documents)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (StreamWriter sw = new StreamWriter(ms, System.Text.Encoding.UTF8))
                    {
                        var jsonData = JsonSerializer.Serialize(BsonTypeMapper.MapToDotNetValue(document), new JsonSerializerOptions()
                        {
                            WriteIndented = true
                        });
                        sw.Write(jsonData);
                        sw.Flush();
                        yield return new DataFile() { FileName = $"{controlId}.dat", Version = document["Version"].AsString, Bytes = ms.ToArray(), Type = "ControlFile" };

                    }
                }
            }

            //Get all the versions of the images
            var filter = CreateImageFilter(applicationId, controlId);
            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var options = new GridFSFindOptions
            {
                Sort = sort
            };
            using (var cursor = await imageBucket.FindAsync(filter, new GridFSFindOptions()))
            {
                var imageFiles = await cursor.ToListAsync();
                foreach (var imageFile in imageFiles)
                {
                    var imageBytes = await imageBucket.DownloadAsBytesAsync(imageFile.Id);
                    yield return new DataFile() { FileName = imageFile.Filename, Version = imageFile.Metadata["version"].AsString, Bytes = imageBytes, Type = "ControlImage" };
                }
            }

        }

        ///<inheritdoc/>
        public async Task<IEnumerable<object>> GetAllControlsForApplication(string applicationId, DateTime laterThan)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotEmpty();
            var controlFilter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationId"], applicationId) & Builders<BsonDocument>.Filter.Gt(x => x["LastUpdated"], laterThan);
            var controls = (await (await controlsCollection.FindAsync<BsonDocument>(controlFilter)).ToListAsync()).Select( s => BsonTypeMapper.MapToDotNetValue(s));
            return controls;
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<ControlImageDataFile>> GetAllControlImagesForApplication(string applicationId, DateTime laterThan)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotEmpty();
            List<ControlImageDataFile> controlImages = new();
            var imageFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["applicationId"], applicationId) & Builders<GridFSFileInfo>.Filter.Gt(x => x.UploadDateTime, laterThan);
            using (var cursor = await imageBucket.FindAsync(imageFilter, new GridFSFindOptions()))
            {
                var imageFiles = await cursor.ToListAsync();
                
                foreach (var imageFile in imageFiles)
                {
                    var imageBytes = await imageBucket.DownloadAsBytesAsync(imageFile.Id);
                    controlImages.Add(new ControlImageDataFile() 
                            { 
                                FileName = imageFile.Filename, 
                                ControlId = imageFile.Metadata["controlId"].AsString, 
                                Version = imageFile.Metadata["version"].AsString,
                                Bytes = imageBytes 
                            });
                }              
            }
            return controlImages;
        }


        ///<inheritdoc/>
        public async Task AddControl(object controlData)
        {
            Guard.Argument(controlData, nameof(controlData)).NotNull();
            if (BsonDocument.TryParse(controlData.ToString(), out BsonDocument document))
            {
                string applicationId = document["ApplicationId"].AsString;
                string controlId = document["ControlId"].AsString;
                string controlVersion = document["Version"].AsString;
                //It is possible to create a revision of control which will also add a new control with same ControlId but different version
                if (await CheckControlVersionExists(applicationId, controlId, controlVersion))
                {
                    throw new InvalidOperationException($"Controld with Id : {controlId} already exists");
                }
                document.Add("LastUpdated", DateTime.UtcNow);
                document.Add("Revision", 1);
                await controlsCollection.InsertOneAsync(document);                
                return;
            }
            throw new ArgumentException("Failed to parse control data in to BsonDocument");
        }

        ///<inheritdoc/>
        public async Task UpdateControl(object controlData)
        {
            Guard.Argument(controlData, nameof(controlData)).NotNull();
       
            if (BsonDocument.TryParse(controlData.ToString(), out BsonDocument document))
            {
                string applicationId = document["ApplicationId"].AsString;
                string controlId = document["ControlId"].AsString;             
                if (!await CheckControlExists(applicationId, controlId))
                {
                    throw new InvalidOperationException($"Controld with Id : {controlId} already exists");
                }
                if(await IsControlDeleted(applicationId, controlId))
                {
                    throw new InvalidOperationException($"Controld with Id : {controlId} is marked deleted");
                }    
                var filter = CreateControlFilter(applicationId, controlId);
                var updateDefinition = Builders<BsonDocument>.Update
                 .Set(x => x["ControlName"], document["ControlName"])
                 .Set(x => x["GroupName"], document["GroupName"])
                 .Set(x => x["ControlImage"], document["ControlImage"])
                 .Set(x => x["ControlDetails"], document["ControlDetails"])
                 .Set(x => x["IsDeleted"], document["IsDeleted"])
                 .Set(x => x["LastUpdated"], DateTime.UtcNow)
                 .Inc(x => x["Revision"], 1);
                var result = await controlsCollection.FindOneAndUpdateAsync(filter, updateDefinition);
                logger.LogInformation("Control : '{0}' was updated with result : '{1}'", document["ControlName"].AsString, result);
                return;
            }

            throw new ArgumentException("Failed to parse control data in to BsonDocument");
        }

        ///<inheritdoc/>
        public async Task<bool> IsControlDeleted(string applicationId, string controlId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationId"], applicationId) &
                            Builders<BsonDocument>.Filter.Eq(x => x["ControlId"], controlId) &
                             Builders<BsonDocument>.Filter.Eq(x => x["IsDeleted"], true);
            long count = await this.controlsCollection.CountDocumentsAsync(filter, new CountOptions() { Limit = 1 });
            return count > 0;
        }

        ///<inheritdoc/>
        public async Task DeleteControlAsync(string applicationId, string controlId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull();
            Guard.Argument(controlId, nameof(controlId)).NotNull();
            
            var filter = Builders<ProjectReferences>.Filter.ElemMatch(x => x.ControlReferences,
                Builders<ControlReference>.Filter.Eq(x => x.ApplicationId, applicationId) &
                Builders<ControlReference>.Filter.Eq(x => x.ControlId, controlId));
            long count = await this.referencesCollection.CountDocumentsAsync(filter);
            if(count > 0)
            {
                throw new InvalidOperationException("Control is in use across one or more projects");
            }
            var updateDefinition = Builders<BsonDocument>.Update.Set("IsDeleted", true)
                .Set("LastUpdated", DateTime.UtcNow);
            var result = await controlsCollection.UpdateManyAsync(CreateControlFilter(applicationId, controlId), updateDefinition);
            logger.LogInformation("Control was marked deleted : {result}", result);
        }

        ///<inheritdoc/>
        public async Task DeleteAllControlsForApplicationAsync(string applicationId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull();
          
            var filter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationId"], applicationId);
            var updateDefinition = Builders<BsonDocument>.Update.Set("IsDeleted", true)
              .Set("LastUpdated", DateTime.UtcNow);
            var result = await controlsCollection.UpdateManyAsync(filter, updateDefinition);
            logger.LogInformation("{0} controls were marked deleted for application with Id : '{1}'", result.ModifiedCount, applicationId);
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
                    {"controlId" , imageMetaData.ControlId },
                    {"version", imageMetaData.Version.ToString() }
                }
            });           
        }

        ///<inheritdoc/>
        public async Task DeleteImageAsync(ControlImageMetaData imageMetaData)
        {
            Guard.Argument(imageMetaData, nameof(imageMetaData)).NotNull();
            var filter = CreateImageFilter(imageMetaData.FileName, imageMetaData.ApplicationId, imageMetaData.ControlId, imageMetaData.Version.ToString());
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

            var updateDefinition = Builders<BsonDocument>.Update.Set("LastUpdated", DateTime.UtcNow);
            await controlsCollection.FindOneAndUpdateAsync<BsonDocument>(CreateControlFilter(imageMetaData.ApplicationId, imageMetaData.ControlId, imageMetaData.Version), updateDefinition);
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
        /// Create filter condition for control file with given applicationId and controlId
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        private FilterDefinition<BsonDocument> CreateControlFilter(string applicationId, string controlId, string version)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq(x => x["ApplicationId"], applicationId);
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x["ControlId"], controlId));
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x["Version"], version));
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

        /// <summary>
        /// Create filter condition for image with given applicationId and controlId.        
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        private FilterDefinition<GridFSFileInfo> CreateImageFilter(string imageName, string applicationId, string controlId, string version)
        {
            var filterBuilder = Builders<GridFSFileInfo>.Filter;
            var filter = filterBuilder.Eq(x => x.Filename, imageName);
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.Metadata["applicationId"], applicationId));
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.Metadata["controlId"], controlId));
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.Metadata["version"], version));
            return filter;
        }
    }
}
