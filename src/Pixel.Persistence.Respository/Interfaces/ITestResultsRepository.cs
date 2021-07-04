using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public interface ITestResultsRepository
    {
        /// <summary>
        /// Get all the test results captured for a test session
        /// </summary>
        /// <param name="sessionId">SessionId of the Session whose tests needs to be retreived</param>
        /// <returns>Collection of <see cref="TestResult"/> belonging to a <see cref="TestSession"/></returns>
        Task<IEnumerable<TestResult>> GetTestResultsAsync(string sessionId);


        Task<IEnumerable<TestResult>> GetTestResultsAsync(TestResultRequest queryParameter);

        Task<long> GetCountAsync(TestResultRequest queryParameter);

        /// <summary>
        /// Store the <see cref="TestResult"/> data to database
        /// </summary>
        /// <param name="testResult"></param>
        /// <returns></returns>
        Task AddTestResultAsync(TestResult testResult);

        /// <summary>
        /// Update the failure reason for a failed test case
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task UpdateFailureReasonAsync(UpdateFailureReasonRequest request);

        /// <summary>
        /// Mark TestResult as processed given it's sessionId and testId
        /// </summary>
        /// <param name="sessionId">SessionId of the TestResult that needs to be updated</param>
        /// <param name="testId">TestId of the TestResult that needs to be updated</param>
        /// <returns></returns>
        Task MarkTestProcessedAsync(string sessionId, string testId);
    }
}
