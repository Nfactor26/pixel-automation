using Pixel.Automation.Core.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface IControlRepositoryClient
    {
        /// <summary>
        /// Get all the controls modified since specified time for a given application
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="laterThan"></param>
        /// <returns></returns>
        Task<IEnumerable<ControlDescription>> GetControls(string applicationId, DateTime laterThan);

        /// <summary>
        /// Get all the control images modified since specified time for a given applications
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="laterThan"></param>
        /// <returns></returns>
        Task<IEnumerable<Core.Models.ControlImageDataFile>> GetControlImages(string applicationId, DateTime laterThan);

        /// <summary>
        /// Add or upate a control 
        /// </summary>
        /// <param name="controlDescription"></param>     
        /// <returns></returns>
        Task AddOrUpdateControl(ControlDescription controlDescription);

        /// <summary>
        /// Delete control
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <returns></returns>
        Task DeleteControl(ControlDescription controlDescription);

        /// <summary>
        /// Add or update control image  at a given resolution for a control
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <param name="imageFile"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        Task AddOrUpdateControlImage(ControlDescription controlDescription, string imageFile);

        /// <summary>
        /// Delete specified control image for a control
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        Task DeleteControlImageAsync(ControlDescription controlDescription, string imageFile);
    }
}
