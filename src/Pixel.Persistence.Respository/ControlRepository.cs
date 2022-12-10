﻿using Dawn;
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
        private readonly IMongoCollection<BsonDocument> controlsCollection;
        private readonly IMongoCollection<ProjectReferences> referencesCollection;
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
            referencesCollection = database.GetCollection<ProjectReferences>(dbSettings.ProjectReferencesCollectionName);
            imageBucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = dbSettings.ImagesBucketName
            });

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
        public async Task AddOrUpdateControl(string controlDataJson)
        {
            Guard.Argument(controlDataJson, nameof(controlDataJson)).NotNull().NotEmpty();
       
            if (BsonDocument.TryParse(controlDataJson, out BsonDocument document))
            {
                string applicationId = document["ApplicationId"].AsString;
                string controlId = document["ControlId"].AsString;
                string version = document["Version"].AsString;

                document.Add("LastUpdated", DateTime.Now.ToUniversalTime());            
              
                //if document already exists, replace it
                if (controlsCollection.CountDocuments<BsonDocument>(x => x["ApplicationId"].Equals(applicationId) 
                    && x["ControlId"].Equals(controlId) && x["Version"].Equals(version), new CountOptions { Limit = 1 }) > 0)
                {
                    await controlsCollection.FindOneAndReplaceAsync<BsonDocument>(CreateControlFilter(applicationId, controlId, version), document);
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
            var updateDefinition = Builders<BsonDocument>.Update.Set("IsDeleted", true);
            await controlsCollection.FindOneAndUpdateAsync<BsonDocument>(CreateControlFilter(applicationId, controlId), updateDefinition);          
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
