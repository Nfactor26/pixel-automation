using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface IApplicationRepositoryClient
    {
        /// <summary>
        /// Get the application file 
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        Task<ApplicationDescription> GetApplication(string applicationId);

        /// <summary>
        /// Get all the specified application files
        /// </summary>
        /// <param name="applicationsToDownload"></param>
        /// <returns></returns>
        Task<IEnumerable<ApplicationDescription>> GetApplications(IEnumerable<ApplicationMetaData> applicationsToDownload);

        /// <summary>
        /// Add or update application file
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <param name="applicationFile"></param>
        /// <returns></returns>
        Task<ApplicationDescription> AddOrUpdateApplication(ApplicationDescription applicationDescription, string applicationFile);

    }
}
