using Dawn;
using MongoDB.Driver;
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
        
        public TestResultsRepository(IMongoDbSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            testResults = database.GetCollection<TestResult>(dbSettings.TestResultsCollectionName);
        }

        public async Task<IEnumerable<TestResult>> GetTestResultsAsync(TestResultRequest queryParameter)
        {
            Guard.Argument(queryParameter).NotNull();

            var filter = GetFilterCriteria(queryParameter);     
           
            if(queryParameter.SortDirection != Core.Enums.SortDirection.None && !string.IsNullOrEmpty(queryParameter.SortBy))
            {
                var sort = (queryParameter.SortDirection == Core.Enums.SortDirection.Ascending) ? Builders<TestResult>.Sort.Ascending(queryParameter.SortBy ?? nameof(TestResult.ExecutionOrder)) :
                Builders<TestResult>.Sort.Descending(queryParameter.SortBy ?? nameof(TestResult.ExecutionOrder));
                var all = testResults.Find(filter).Sort(sort).Skip(queryParameter.Skip).Limit(queryParameter.Take);
                var result = await all.ToListAsync();
                return result ?? Enumerable.Empty<TestResult>();
            }
            else
            {
                var defaultSort = Builders<TestResult>.Sort.Ascending(nameof(TestResult.ExecutionOrder));
                var all = testResults.Find(filter).Sort(defaultSort).Skip(queryParameter.Skip).Limit(queryParameter.Take);
                var result = await all.ToListAsync();
                return result ?? Enumerable.Empty<TestResult>();
            }
          
        }

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

        public async Task<IEnumerable<TestResult>> GetTestResultsAsync(string sessionId)
        {
            Guard.Argument(sessionId).NotNull().NotEmpty();
            var result = await testResults.FindAsync<TestResult>(s => s.SessionId.Equals(sessionId));
            return await result.ToListAsync();
        }
        
        public async Task AddTestResultAsync(TestResult testResult)
        {
            Guard.Argument(testResult).NotNull();
            await testResults.InsertOneAsync(testResult);
        }

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

        public async Task MarkTestProcessedAsync(string sessionId, string testId)
        {
            var filterBuilder = Builders<TestResult>.Filter;
            var filter = filterBuilder.And(filterBuilder.Eq(t => t.SessionId, sessionId), filterBuilder.Eq(t => t.TestId, testId));
            var updateBuilder = Builders<TestResult>.Update;
            var update = updateBuilder.Set(t => t.IsProcessed, true);
            await testResults.FindOneAndUpdateAsync(filter, update);
        }       
    }
}
