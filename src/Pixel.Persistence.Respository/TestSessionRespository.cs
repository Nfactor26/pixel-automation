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
    public class TestSessionRespository : ITestSessionRepository
    {
        private readonly IMongoCollection<TestSession> sessionsCollection;

        public TestSessionRespository(IMongoDbSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            sessionsCollection = database.GetCollection<TestSession>(dbSettings.SessionsCollectionName);
        }

        public async Task<TestSession> GetTestSessionAsync(string sessionId)
        {
            Guard.Argument(sessionId).NotNull().NotEmpty();
            var result = await sessionsCollection.FindAsync<TestSession>(s => s.Id.Equals(sessionId));
            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<TestSession>> GetTestSessionsAsync()
        {
            var all = await sessionsCollection.FindAsync(s => true);
            return await all.ToListAsync();
        }

        public async Task<IEnumerable<TestSession>> GetTestSessionsAsync(TestSessionRequest queryParameter)
        {
            Guard.Argument(queryParameter).NotNull();

            var filterBuilder = Builders<TestSession>.Filter;
            var filter = filterBuilder.Gt(t => t.SessionStartTime, queryParameter.ExecutedAfter.ToUniversalTime());
            if(!string.IsNullOrEmpty(queryParameter.ProjectName))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(t => t.ProjectName, queryParameter.ProjectName));
            }
            if(!string.IsNullOrEmpty(queryParameter.MachineName))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(t => t.MachineName, queryParameter.MachineName));
            }
            if (!string.IsNullOrEmpty(queryParameter.TemplateName))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(t => t.TemplateName, queryParameter.TemplateName));
            }
            var sort = Builders<TestSession>.Sort.Descending(queryParameter.OrderBy ?? nameof(TestSession.SessionStartTime));
            var all =  sessionsCollection.Find(filter).Sort(sort).Skip(queryParameter.Skip).Limit(queryParameter.Take);
            var result =  await all.ToListAsync();
            return result ?? Enumerable.Empty<TestSession>();
        }

        public async Task<long> GetCountAsync(TestSessionRequest queryParameter)
        {
            Guard.Argument(queryParameter).NotNull();

            var filterBuilder = Builders<TestSession>.Filter;
            var filter = filterBuilder.Gt(t => t.SessionStartTime, queryParameter.ExecutedAfter.ToUniversalTime());
            if (!string.IsNullOrEmpty(queryParameter.ProjectName))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(t => t.ProjectName, queryParameter.ProjectName));
            }
            if (!string.IsNullOrEmpty(queryParameter.MachineName))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(t => t.MachineName, queryParameter.MachineName));
            }
            if (!string.IsNullOrEmpty(queryParameter.TemplateName))
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(t => t.TemplateName, queryParameter.TemplateName));
            }
            return await sessionsCollection.CountDocumentsAsync(filter);
        }

        public async Task<IEnumerable<string>> GetUnprocessedSessionsAsync()
        {
            var filterBuilder = Builders<TestSession>.Filter;
            var condition = filterBuilder.And(filterBuilder.Ne<SessionStatus>(s => s.SessionStatus , Core.Enums.SessionStatus.InProgress), filterBuilder.Eq(s => s.IsProcessed , false) );
            var fields = Builders<TestSession>.Projection.Include(p => p.Id);
            var all = await sessionsCollection.Find(condition).Project<TestSession>(fields).ToListAsync();        
            return all.Select(s => s.Id);
        }

        public async Task MarkSessionProcessedAsync(string sessionId)
        {
            var filter = Builders<TestSession>.Filter.Eq(s => s.Id, sessionId);
            var updateBuilder = Builders<TestSession>.Update;
            var update = updateBuilder.Set(t => t.IsProcessed, true);
            await sessionsCollection.FindOneAndUpdateAsync(filter, update);
        }    
      
        public async Task AddTestSessionAsync(TestSession testSession)
        {
            Guard.Argument(testSession).NotNull();
            await sessionsCollection.InsertOneAsync(testSession);
        }

        public async Task UpdateTestSessionAsync(string sessionId, TestSession testSession)
        {
            Guard.Argument(sessionId).NotNull().NotEmpty();
            Guard.Argument(testSession).NotNull();
            var filter = Builders<TestSession>.Filter.Eq(t => t.Id, sessionId);
            await sessionsCollection.ReplaceOneAsync(filter, testSession);
        }

        public async Task<bool> TryDeleteTestSessionAsync(string sessionId)
        {
            Guard.Argument(sessionId).NotNull().NotEmpty();
            var result = await sessionsCollection.DeleteOneAsync(s => s.Id.Equals(sessionId));
            return result.DeletedCount == 1;
        }

      
    }
}
