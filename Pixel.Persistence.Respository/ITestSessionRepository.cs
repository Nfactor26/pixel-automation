using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public interface ITestSessionRepository
    {
        Task<TestSession> GetTestSessionAsync(string sessionId);

        Task<IEnumerable<TestSession>> GetTestSessionsAsync();

        Task AddTestSessionAsync(TestSession testSession);

        Task DeleteApplicationAsync(string sessionId);
       
    }
}
