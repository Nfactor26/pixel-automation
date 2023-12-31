using Pixel.Automation.Core.Controls;
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
        /// Add new application description
        /// </summary>
        /// <param name="applicationDescription"></param>     
        /// <returns></returns>
        Task AddApplication(ApplicationDescription applicationDescription);

        /// <summary>
        /// Update an existing application description
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        Task UpdateApplication(ApplicationDescription applicationDescription);

        /// <summary>
        /// Mark the application as deleted
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        Task DeleteApplicationAsync(ApplicationDescription applicationDescription);

        /// <summary>
        /// Add a new screen to the application
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="applicationScreen"></param>
        /// <returns></returns>
        Task AddApplicationScreen(string applicationId, ApplicationScreen applicationScreen);

        /// <summary>
        /// Rename an existing screen for the application
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="screenId"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        Task RenameApplicationScreen(string applicationId, string screenId, string newName);
               
        /// <summary>
        /// Move control from one screen to another
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <param name="targetScreenId"></param>
        /// <returns></returns>
        Task MoveControlToScreen(ControlDescription controlDescription, string targetScreenId);

        /// <summary>
        /// Move prefab from one screen to another
        /// </summary>
        /// <param name="prefabProject"></param>
        /// <param name="targetScreenId"></param>
        /// <returns></returns>
        Task MovePrefabToScreen(PrefabProject prefabProject, string targetScreenId);

    }
}
