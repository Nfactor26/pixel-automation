using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    /// <summary>
    /// ITestStatisticsRepository is used to manage <see cref="TestStatistics"/> stored in database
    /// </summary>
    public interface ITestStatisticsRepository
    {
        /// <summary>
        /// For a session, process all the results and update statistics
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task AddOrUpdateStatisticsAsync(string sessionId);

        /// <summary>
        /// For a TestResult , create or update the Test statistics
        /// </summary>
        /// <param name="testResult"></param>
        /// <param name="fromTime"></param>
        /// <param name="toTime"></param>
        /// <returns></returns>
        Task AddOrUpdateStatisticsAsync(IEnumerable<TestResult> testResult, string projectVersion, DateTime fromTime, DateTime toTime);

        /// <summary>
        /// Get the TestStatistics for a test case whose Id is provided
        /// </summary>
        /// <param name="testCaseId">Id of the test case whose statistics is required</param>
        /// <returns><see cref="TestStatistics"/> for a test case with a given Id</returns>
        Task<TestStatistics> GetTestStatisticsByIdAsync(string testCaseId);

        /// <summary>
        /// Get the TestStatistics for a test case whose Name is provided
        /// </summary>
        /// <param name="testCaseName">Name of the test case whose statistics is required</param>
        /// <returns><see cref="TestStatistics"/> for a test case with a given Name</returns>
        Task<TestStatistics> GetTestStatisticsByNameAsync(string testCaseName);
    }
}
