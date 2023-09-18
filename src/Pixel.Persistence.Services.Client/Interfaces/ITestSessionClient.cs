using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
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
        /// Add a new test result executed in a given session.
        /// Identifier of the test result is returned.
        /// </summary>
        /// <param name="testResult"></param>
        /// <returns></returns>
        Task<TestResult> AddResultAsync(TestResult testResult);

        /// <summary>
        /// Add a trace image file for a test result
        /// </summary>
        /// <param name="testResult"></param>
        /// <param name="imageFiles"></param>
        /// <returns></returns>
        Task AddTraceImagesAsync(TestResult testResult, IEnumerable<string> imageFiles);
    }
}
