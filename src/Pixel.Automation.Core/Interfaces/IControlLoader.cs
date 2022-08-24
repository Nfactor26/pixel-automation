using Pixel.Automation.Core.Controls;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// IControlLoader provides a contract for retrieving <see cref="ControlDescription"/> data.
    /// </summary>
    public interface IControlLoader
    {
        /// <summary>
        /// Load ControlDescription for specified applicationId and controlId.
        /// ControlVersion will be determined from ControlReferences file for the project.
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        ControlDescription GetControl(string applicationId, string controlId);

        /// <summary>
        /// Remove a control from cache if it exists.
        /// </summary>        
        /// <param name="controlId"></param>
        void RemoveFromCache(string controlId);
    }
}
