using Pixel.Automation.Core.Models;
using System;
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
        /// Get all the applications that have been modified since specified datetime
        /// </summary>
        /// <param name="applicationsToDownload"></param>
        /// <returns></returns>
        Task<IEnumerable<ApplicationDescription>> GetApplications(DateTime laterThan);

        /// <summary>
        /// Add or update application description
        /// </summary>
        /// <param name="applicationDescription"></param>     
        /// <returns></returns>
        Task AddOrUpdateApplication(ApplicationDescription applicationDescription);

    }
}
