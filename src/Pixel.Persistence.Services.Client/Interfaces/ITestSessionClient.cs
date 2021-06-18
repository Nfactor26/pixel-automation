using Pixel.Persistence.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface ITestSessionClient
    {
        /// <summary>
        /// Save details of the TestSession in to Db
        /// </summary>
        /// <param name="testSession"></param>
        /// <returns>SessionId of the new TestSession</returns>
        Task<string> AddSessionAsync(TestSession testSession);

        /// <summary>
        /// Update the TestSession details
        /// </summary>
        /// <param name="sessionId">SessionId of the TestSession to be updated</param>
        /// <param name="testSession">Updated TestSession data</param>
        /// <returns></returns>
        Task UpdateSessionAsync(string sessionId, TestSession testSession);

        /// <summary>
        /// Add a new test result executed in a given session
        /// </summary>
        /// <param name="testResult"></param>
        /// <returns></returns>
        Task AddResultAsync(TestResult testResult);
    }
}
