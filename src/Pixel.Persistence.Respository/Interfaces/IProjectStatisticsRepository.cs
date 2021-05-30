using Pixel.Persistence.Core.Models;
using System;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    interface IProjectStatisticsRepository
    {
        /// <summary>
        /// Create or Update TestStatistics for a given TestResult
        /// </summary>
        /// <param name="testResult"><see cref="TestResult"/> whose statistics needs to be updated</param>
        /// <returns></returns>
        Task AddOrUpdateStatisticsAsync(TestResult testResult, DateTime fromTime, DateTime toTime);

        /// <summary>
        /// Get ProjectStatistics given a projectId
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<ProjectStatistics> GetProjectStatisticsByIdAsync(string projectId);

        /// <summary>
        /// Get ProjectStatistics given a projectName
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        Task<ProjectStatistics> GetProjectStatisticsByNameAsync(string projectName);
    }
}
