using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public interface IControlRepository
    {
        /// <summary>
        /// Get collection of <see cref="ControlMetaData"/> for a given application whose applicationId is specified
        /// </summary>
        /// <param name="applicationId">ApplicationId of the application</param>
        /// <returns></returns>
        IAsyncEnumerable<ControlMetaData> GetMetadataAsync(string applicationId);

        /// <summary>
        /// Get the Control data 
        /// </summary>
        /// <param name="applicationId">ApplicationId of the Control</param>
        /// <param name="controlId">ControlId of the Control</param>
        /// <returns></returns>
        IAsyncEnumerable<DataFile> GetControlFiles(string applicationId, string controlId);
   
        /// <summary>
        /// Add or Update Control data
        /// </summary>
        /// <param name="control"><see cref="ControlMetaData"/> of the Control</param>
        /// <param name="fileName">FileName to store data to</param>
        /// <param name="fileData">Contents of the file</param>
        /// <returns></returns>
        Task AddOrUpdateControl(ControlMetaData control, string fileName, byte[] fileData);

        /// <summary>
        /// Add or Update Control image
        /// </summary>
        /// <param name="imageMetaData"><see cref="ControlImageMetaData"/> of the Control Image</param>
        /// <param name="fileName">FileName to store image to</param>
        /// <param name="fileData">Content of the control image file </param>
        /// <returns></returns>
        Task AddOrUpdateControlImage(ControlImageMetaData imageMetaData, string fileName, byte[] fileData);
    }
}
