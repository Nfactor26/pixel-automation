using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    /// <summary>
    /// ITestResultsRepository is used to manage the <see cref="TestResult"/> stored in database
    /// </summary>
    public interface ITestResultsRepository
    {
        /// <summary>
        /// Get TestResult with a given identifer
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TestResult> GetTestResultAsync(string id);

        /// <summary>
        /// Get all the test results captured for a test session
        /// </summary>
        /// <param name="sessionId">SessionId of the Session whose tests needs to be retreived</param>
        /// <returns>Collection of <see cref="TestResult"/> belonging to a <see cref="TestSession"/></returns>
        Task<IEnumerable<TestResult>> GetTestResultsForSessionAsync(string sessionId);

        /// <summary>
        /// Get all the <see cref="TestResult"/> matching the criteria specified by the query
        /// </summary>
        /// <param name="queryParameter"></param>
        /// <returns></returns>
        Task<IEnumerable<TestResult>> GetTestResultsAsync(TestResultRequest queryParameter);

        /// <summary>
        /// Get the count of <see cref="TestResult"/> matching the criteria specified by the query
        /// </summary>
        /// <param name="queryParameter"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Save trace image file for a given test result in to db
        /// </summary>
        /// <param name="traceImageMetaData"></param>
        /// <param name="fileName"></param>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        Task AddTraceImage(TraceImageMetaData traceImageMetaData, string fileName, byte[] imageBytes);

        /// <summary>
        /// Get trace image for a given test result and file name
        /// </summary>
        /// <param name="testResultId"></param>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        Task<DataFile> GetTraceImage(string testResultId, string imageFile);

        /// <summary>
        /// Get all the trace image files for a given test result
        /// </summary>
        /// <param name="testResultId"></param>
        /// <returns></returns>
        Task<IEnumerable<DataFile>> GetTraceImages(string testResultId);
    }
}
