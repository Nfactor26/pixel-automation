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
        Task DownloadApplicationsWithControlsAsync();


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
        /// Add a new application
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        Task AddApplicationAsync(ApplicationDescription applicationDescription);

        /// <summary>
        /// Update an existing application
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        Task UpdateApplicationAsync(ApplicationDescription applicationDescription);

        /// <summary>
        /// Mark the application as deleted
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        Task DeleteApplicationAsync(ApplicationDescription applicationDescription);

        /// <summary>
        /// Save application description to disk
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        string SaveApplicationToDisk(ApplicationDescription application);

        /// <summary>
        /// Add a new screen to the application
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <param name="applicationScreen"></param>
        /// <returns></returns>
        Task AddApplicationScreen(ApplicationDescription applicationDescription, ApplicationScreen applicationScreen);

        /// <summary>
        /// Rename an existing application screen
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <param name="screen"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        Task RenameApplicationScreen(ApplicationDescription applicationDescription, ApplicationScreen screen, string newName);

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
        /// Get all the versions of control given it's applicationId and controlId
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        IEnumerable<ControlDescription> GetAllVersionsOfControl(string applicationId, string controlId);

        /// <summary>
        /// Add a new control to specified screen of the applicationS
        /// </summary>       
        /// <param name="controlDescription"></param>
        /// <param name="screenId"></param>
        /// <returns></returns>
        Task AddControlToScreenAsync(ControlDescription controlDescription, string screenId);

        /// <summary>
        /// Move control from one screen to another
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <param name="targetScreenId"></param>
        /// <returns></returns>
        Task MoveControlToScreen(ControlDescription controlDescription, string targetScreenId);

        /// <summary>
        /// Update details of an existing control
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <returns></returns>
        Task UpdateControlAsync(ControlDescription controlDescription);

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