using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface IApplicationRepositoryClient
    {
        /// <summary>
        /// Get the ApplicationDescription for a given applicationId 
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        Task<ApplicationDescription> GetApplication(string applicationId);

        /// <summary>
        /// Get ApplicationDescription for all the specified application
        /// </summary>
        /// <param name="applicationsToDownload"></param>
        /// <returns></returns>
        Task<IEnumerable<ApplicationDescription>> GetApplications(IEnumerable<ApplicationMetaData> applicationsToDownload);

        /// <summary>
        /// Add or update application description
        /// </summary>
        /// <param name="applicationDescription"></param>     
        /// <returns></returns>
        Task AddOrUpdateApplication(ApplicationDescription applicationDescription);

    }
}
