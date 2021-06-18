using System.Collections.Generic;
using System.Threading.Tasks;
using Pixel.Persistence.Core.Models;

namespace Pixel.Persistence.Services.Client
{
    public interface IMetaDataClient
    {
        /// <summary>
        /// Get ApplicationMetaData for most recent saved application data on server
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ApplicationMetaData>> GetApplicationMetaData();
     
        /// <summary>
        /// Get ProjectMetaData for most recent saved project data on server 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProjectMetaData>> GetProjectsMetaData();
        
        /// <summary>
        /// Get ProjectMetaData for most recent saved project data files per version on server for a given projectId
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<IEnumerable<ProjectMetaData>> GetProjectMetaData(string projectId);
    }
}