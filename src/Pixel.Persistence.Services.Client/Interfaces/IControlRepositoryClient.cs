using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface IControlRepositoryClient
    {
        /// <summary>
        /// Get all the requested controls for a given application
        /// </summary>
        /// <param name="controlDataRequest"></param>
        /// <returns></returns>
        Task<byte[]> GetControls(GetControlDataForApplicationRequest controlDataRequest);
      
        /// <summary>
        /// Add or upate a control 
        /// </summary>
        /// <param name="controlDescription"></param>     
        /// <returns></returns>
        Task AddOrUpdateControl(ControlDescription controlDescription);
    
        /// <summary>
        /// Add or update control image  at a given resolution for a control
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <param name="imageFile"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        Task AddOrUpdateControlImage(ControlDescription controlDescription, string imageFile, string resolution);
    }
}
