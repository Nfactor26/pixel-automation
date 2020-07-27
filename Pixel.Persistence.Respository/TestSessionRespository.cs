using Dawn;
using MongoDB.Driver;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class TestSessionRespository : ITestSessionRepository
    {
        private readonly IMongoCollection<TestSession> sessions;

        public TestSessionRespository(IMongoDbSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            sessions = database.GetCollection<TestSession>(dbSettings.SessionsCollectionName);
        }

        public async Task<TestSession> GetTestSessionAsync(string sessionId)
        {
            Guard.Argument(sessionId).NotNull().NotEmpty();
            var result = await sessions.FindAsync<TestSession>(s => s.SessionId.Equals(sessionId));
            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<TestSession>> GetTestSessionsAsync()
        {
            var all = await sessions.FindAsync(s => true);
            return await all.ToListAsync();
        }

        public async Task AddTestSessionAsync(TestSession testSession)
        {
            Guard.Argument(testSession).NotNull();
            await sessions.InsertOneAsync(testSession);
        }

        public async Task DeleteTestSessionAsync(string sessionId)
        {
            Guard.Argument(sessionId).NotNull().NotEmpty();
            await sessions.DeleteOneAsync(s => s.SessionId.Equals(sessionId));
        }
    }
}
