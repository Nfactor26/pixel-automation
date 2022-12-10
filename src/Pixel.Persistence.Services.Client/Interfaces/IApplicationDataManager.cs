using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Models;

namespace Pixel.Persistence.Services.Client
{ 
    public interface IApplicationDataManager
    {
        /// <summary>
        /// Delete application and automation data directory from local disk
        /// </summary>
        void CleanLocalData();

        /// <summary>
        /// Load all application description from disk (e.g. for use in application explorer) and return loaded application descriptions
        /// </summary>
        /// <returns></returns>
        IEnumerable<ApplicationDescription> GetAllApplications();


        /// <summary>
        /// Download most recent version of application and controls
        /// </summary>
        /// <returns></returns>
        Task UpdateApplicationRepository();


        /// <summary>
        /// Download most recent version of all the applications
        /// </summary>
        /// <returns></returns>
        Task DownloadApplicationsAsync();

        /// <summary>
        /// Download most recent version of all the controls and their images for a given application
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        Task DownloadControlsAsync(string applicationId);

        /// <summary>
        /// Add or update a application file
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        Task AddOrUpdateApplicationAsync(ApplicationDescription applicationDescription);
                     

        /// <summary>
        /// Load all the control files from disk for a given application (e.g. for use in control explorer) and return loaded control descriptions
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        IEnumerable<ControlDescription> GetAllControls(ApplicationDescription applicationDescription);

        /// <summary>
        /// Load all the control files from disk for a given screen of a given application. Only latest revision is included.
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <param name="screenName"></param>
        /// <returns></returns>
        IEnumerable<ControlDescription> GetControlsForScreen(ApplicationDescription applicationDescription, string screenName);

        /// <summary>
        /// Get all the revisions of control given it's applicationId and controlId
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        IEnumerable<ControlDescription> GetControlsById(string applicationId, string controlId);

        /// <summary>
        /// Add or update a control file
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <returns></returns>
        Task AddOrUpdateControlAsync(ControlDescription controlDescription);

        /// <summary>
        /// Delete a given control
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <returns></returns>
        Task DeleteControlAsync(ControlDescription controlDescription);

        /// <summary>
        /// Add or update control image for control
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <param name="stream"></param>      
        /// <returns></returns>
        Task<string> AddOrUpdateControlImageAsync(ControlDescription controlDescription, Stream stream);

        /// <summary>
        /// Delete specified imageFile for control
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        Task DeleteControlImageAsync(ControlDescription controlDescription, string imageFile);
       
    }
}