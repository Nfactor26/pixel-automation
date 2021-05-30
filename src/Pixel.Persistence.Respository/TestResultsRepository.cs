using Dawn;
using MongoDB.Driver;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
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

        public async Task AddTestResultAsync(TestResult testResult)
        {
            Guard.Argument(testResult).NotNull();
            await testResults.InsertOneAsync(testResult);
        }

        public async Task<IEnumerable<TestResult>> GetTestResultsAsync(string sessionId)
        {
            Guard.Argument(sessionId).NotNull().NotEmpty();
            var result = await testResults.FindAsync<TestResult>(s => s.SessionId.Equals(sessionId));
            return await result.ToListAsync();
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
