using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public interface IProjectStatisticsRepository
    {        
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

        /// <summary>
        /// Update ProjectStatistics for a given Session
        /// </summary>
        /// <param name="sessionId"><see cref="string"/>Id of the session that needs to be processed for project statistics</param>
        /// <returns></returns>
        Task AddOrUpdateStatisticsAsync(string sessionId);

    }
}
