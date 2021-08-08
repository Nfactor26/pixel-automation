using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    /// <summary>
    /// IControlRepository is used to manage the ControlDescription data stored in database.
    /// This also includes the images associated with the Control.
    /// </summary>
    public interface IControlRepository
    {
        /// <summary>
        /// Get collection of <see cref="ControlMetaData"/> for a given application whose applicationId is specified.
        /// ControlMetaData is used on client side to determine if there are any newer files available on the server.
        /// </summary>
        /// <param name="applicationId">ApplicationId of the application</param>
        /// <returns></returns>
        IAsyncEnumerable<ControlMetaData> GetMetadataAsync(string applicationId);

        /// <summary>
        /// Get the ControlDescription data along with the image files for the control
        /// </summary>
        /// <param name="applicationId">ApplicationId of the Control</param>
        /// <param name="controlId">ControlId of the Control</param>
        /// <returns>A collection of DataFile one for each control description and associate images</returns>
        IAsyncEnumerable<DataFile> GetControlFiles(string applicationId, string controlId);

        /// <summary>
        /// Add or Update Control data
        /// </summary>
        /// <param name="controlDataJson">json representation of the ControlDescription</param>     
        /// <returns></returns>
        Task AddOrUpdateControl(string controlDataJson);

        /// <summary>
        /// Add or Update Control image
        /// </summary>
        /// <param name="imageMetaData"><see cref="ControlImageMetaData"/> of the Control Image</param>
        /// <param name="fileName">FileName to store image to</param>
        /// <param name="fileData">Content of the control image file </param>
        /// <returns></returns>
        Task AddOrUpdateControlImage(ControlImageMetaData imageMetaData, string fileName, byte[] fileData);

        /// <summary>
        /// Delete the image from the database
        /// </summary>
        /// <param name="control">ControlImageMetaData that can be used to identify the image to be deleted</param>
        /// <returns></returns>
        Task DeleteImageAsync(ControlImageMetaData control);
    }
}
