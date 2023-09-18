using Dawn;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Pixel.Persistence.Core.Enums;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class TestResultsRepository : ITestResultsRepository
    {
        private readonly IMongoCollection<TestResult> testResults;
        private readonly IGridFSBucket traceImagesBucket;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dbSettings"></param>
        public TestResultsRepository(IMongoDbSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            testResults = database.GetCollection<TestResult>(dbSettings.TestResultsCollectionName);
            traceImagesBucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = dbSettings.TraceImagesBucketName
            });
        }

        /// <inheritdoc/>  
        public async Task<IEnumerable<TestResult>> GetTestResultsAsync(TestResultRequest queryParameter)
        {
            Guard.Argument(queryParameter).NotNull();

            var filter = GetFilterCriteria(queryParameter);
            var projection = Builders<TestResult>.Projection.Exclude(x => x.Traces);
            if (queryParameter.SortDirection != Core.Enums.SortDirection.None && !string.IsNullOrEmpty(queryParameter.SortBy))
            {
                var sort = (queryParameter.SortDirection == Core.Enums.SortDirection.Ascending) ? Builders<TestResult>.Sort.Ascending(queryParameter.SortBy ?? nameof(TestResult.ExecutionOrder)) :
                Builders<TestResult>.Sort.Descending(queryParameter.SortBy ?? nameof(TestResult.ExecutionOrder));
                var all = testResults.Find(filter).Project<TestResult>(projection).Sort(sort).Skip(queryParameter.Skip).Limit(queryParameter.Take);
                var result = await all.ToListAsync();
                return result ?? Enumerable.Empty<TestResult>();
            }
            else
            {
                var defaultSort = Builders<TestResult>.Sort.Ascending(nameof(TestResult.ExecutionOrder));
                var all = testResults.Find(filter).Project<TestResult>(projection).Sort(defaultSort).Skip(queryParameter.Skip).Limit(queryParameter.Take);
                var result = await all.ToListAsync();
                return result ?? Enumerable.Empty<TestResult>();
            }
          
        }

        /// <inheritdoc/>  
        public async Task<long> GetCountAsync(TestResultRequest queryParameter)
        {
            Guard.Argument(queryParameter).NotNull();
            var filter = GetFilterCriteria(queryParameter);         
            return await testResults.CountDocumentsAsync(filter);
        }

        private FilterDefinition<TestResult> GetFilterCriteria(TestResultRequest queryParameter)
        {
            var filterBuilder = Builders<TestResult>.Filter;
            var filter = filterBuilder.Empty;
            if (!string.IsNullOrEmpty(queryParameter.SessionId))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(t => t.SessionId, queryParameter.SessionId));
            }
            if (!string.IsNullOrEmpty(queryParameter.ProjectId))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(t => t.ProjectId, queryParameter.ProjectId));
            }
            if (!string.IsNullOrEmpty(queryParameter.TestId))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(t => t.TestId, queryParameter.TestId));
            }
            if (queryParameter.Result != default)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(t => t.Result, queryParameter.Result));
            }
            if (!string.IsNullOrEmpty(queryParameter.FixtureName))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(t => t.FixtureName, queryParameter.FixtureName));
            }
            //if (queryParameter.ExecutionTimeGte != default)
            //{
            //    filter = filterBuilder.And(filter, filterBuilder.Gte(t => t.ExecutionTimeGte, queryParameter.ExecutionTimeGte));
            //}
            //if (queryParameter.ExecutionTimeLte != default)
            //{
            //    filter = filterBuilder.And(filter, filterBuilder.Lte(t => t.ExecutionTimeLte, queryParameter.ExecutionTimeLte));
            //}
            if (queryParameter.ExecutedAfter != default)
            {
                filter = filterBuilder.And(filter, filterBuilder.Gte(t => t.ExecutedOn, queryParameter.ExecutedAfter));
            }
            return filter;
        }

        /// <inheritdoc/>  
        public async Task<TestResult> GetTestResultAsync(string id)
        {
            Guard.Argument(id, nameof(id)).NotNull().NotEmpty();
            var result = await testResults.FindAsync<TestResult>(s => s.Id.Equals(id));
            return await result.FirstOrDefaultAsync();
        }

        /// <inheritdoc/>  
        public async Task<IEnumerable<TestResult>> GetTestResultsForSessionAsync(string sessionId)
        {
            Guard.Argument(sessionId).NotNull().NotEmpty();
            var projection = Builders<TestResult>.Projection.Exclude(x => x.Traces);          
            var result = await (testResults.Find(s => s.SessionId.Equals(sessionId)).Project<TestResult>(projection)).ToListAsync();           
            return result;
        }

        /// <inheritdoc/>  
        public async Task AddTestResultAsync(TestResult testResult)
        {
            Guard.Argument(testResult).NotNull();
            await testResults.InsertOneAsync(testResult);
        }

        /// <inheritdoc/>  
        public async Task UpdateFailureReasonAsync(UpdateFailureReasonRequest request)
        {
            Guard.Argument(request).NotNull();
            var filterBuilder = Builders<TestResult>.Filter;
            var filter = filterBuilder.And(filterBuilder.Eq(t => t.TestId, request.TestId),
                filterBuilder.Eq(t => t.SessionId, request.SessionId),
                filterBuilder.Eq(t => t.Result, TestStatus.Failed));
            var updateBuilder = Builders<TestResult>.Update;
            var update = updateBuilder.Set(t => t.FailureDetails.FailureReason, request.FailureReason);
            var result = await testResults.FindOneAndUpdateAsync(filter, update);
        }

        /// <inheritdoc/>  
        public async Task MarkTestProcessedAsync(string sessionId, string testId)
        {
            var filterBuilder = Builders<TestResult>.Filter;
            var filter = filterBuilder.And(filterBuilder.Eq(t => t.SessionId, sessionId), filterBuilder.Eq(t => t.TestId, testId));
            var updateBuilder = Builders<TestResult>.Update;
            var update = updateBuilder.Set(t => t.IsProcessed, true);
            await testResults.FindOneAndUpdateAsync(filter, update);
        }

        /// <inheritdoc/>  
        public async Task AddTraceImage(TraceImageMetaData traceImageMetaData, string fileName, byte[] imageBytes)
        {
            Guard.Argument(traceImageMetaData, nameof(traceImageMetaData)).NotNull();
            Guard.Argument(fileName, nameof(fileName)).NotNull().NotEmpty(); ;
            Guard.Argument(imageBytes, nameof(imageBytes)).NotNull();

            await traceImagesBucket.UploadFromBytesAsync(fileName, imageBytes, new GridFSUploadOptions()
            {
                Metadata = new BsonDocument()
                {
                    {"sesionId" , traceImageMetaData.SessionId},
                    {"testResultId" , traceImageMetaData.TestResultId }
                }
            });
        }

        ///<inheritdoc/>
        public async Task<DataFile> GetTraceImage(string testResultId, string imageFile)
        {
            Guard.Argument(testResultId, nameof(testResultId)).NotNull().NotEmpty();
            Guard.Argument(imageFile, nameof(imageFile)).NotNull().NotEmpty();

            var filterBuilder = Builders<GridFSFileInfo>.Filter;
            var filter = filterBuilder.And(filterBuilder.Eq(x => x.Metadata["testResultId"], testResultId),
                filterBuilder.Eq(x => x.Filename, imageFile));         
           
            using (var cursor = await traceImagesBucket.FindAsync(filter, new GridFSFindOptions() { Limit = 1 }))
            {
                var imageData = (await cursor.ToListAsync()).FirstOrDefault();
                if(imageData != null)
                {
                    var imageBytes = await traceImagesBucket.DownloadAsBytesAsync(imageData.Id);
                    return new ControlImageDataFile()
                    {
                        FileName = imageData.Filename,
                        Bytes = imageBytes
                    };
                }
            }
            throw new GridFSFileNotFoundException(imageFile, 1);
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<DataFile>> GetTraceImages(string testResultId)
        {
            Guard.Argument(testResultId, nameof(testResultId)).NotNull().NotEmpty();
            List<DataFile> controlImages = new();
            var imageFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["testResultId"], testResultId);
            using (var cursor = await traceImagesBucket.FindAsync(imageFilter, new GridFSFindOptions()))
            {
                var imageFiles = await cursor.ToListAsync();

                foreach (var imageFile in imageFiles)
                {
                    var imageBytes = await traceImagesBucket.DownloadAsBytesAsync(imageFile.Id);
                    controlImages.Add(new ControlImageDataFile()
                    {
                        FileName = imageFile.Filename,                        
                        Bytes = imageBytes
                    });
                }
            }
            return controlImages;
        }
    }
}
