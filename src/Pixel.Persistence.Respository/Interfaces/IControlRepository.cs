using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository;

/// <summary>
/// IControlRepository is used to manage the ControlDescription data stored in database.
/// This also includes the images associated with the Control.
/// </summary>
public interface IControlRepository
{ 

    /// <summary>
    /// Get the ControlDescription data along with the image files for the control
    /// </summary>
    /// <param name="applicationId">ApplicationId of the Control</param>
    /// <param name="controlId">ControlId of the Control</param>
    /// <returns>A collection of DataFile one for each control description and associate images</returns>
    IAsyncEnumerable<DataFile> GetControlFiles(string applicationId, string controlId);

    /// <summary>
    /// Get all controls modified since specified time for a given application
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="laterThan"></param>
    /// <returns></returns>
    Task<IEnumerable<object>> GetAllControlsForApplication(string applicationId, DateTime laterThan);

    /// <summary>
    /// Get all the control image modified since specified time for a given application
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="laterThan"></param>
    /// <returns></returns>
    Task<IEnumerable<ControlImageDataFile>> GetAllControlImagesForApplication(string applicationId, DateTime laterThan);

    /// <summary>
    /// Add Control data
    /// </summary>
    /// <param name="controlData"></param>     
    /// <returns></returns>
    Task AddControl(object controlData);

    /// <summary>
    /// Update Control data
    /// </summary>
    /// <param name="controlData"></param>     
    /// <returns></returns>
    Task UpdateControl(object controlData);

    /// <summary>
    /// Check if a control is marked deleted.
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="controlId"></param>
    /// <returns></returns>
    Task<bool> IsControlDeleted(string applicationId, string controlId);

    /// <summary>
    /// Set IsDeleted flag on all versions of control to true.
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="controlId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown if the control is in use acrosss any project</exception>
    Task DeleteControlAsync(string applicationId, string controlId);

    /// <summary>
    /// Mark all the controls as deleted for a given application
    /// </summary>
    /// <param name="applicationId"></param>
    /// <returns></returns>
    Task DeleteAllControlsForApplicationAsync(string applicationId);

    /// <summary>
    /// Add or Update Control image
    /// </summary>
    /// <param name="imageMetaData"><see cref="ControlImageMetaData"/> of the Control Image</param>
    /// <param name="fileName">FileName to store image to</param>
    /// <param name="fileData">Content of the control image file </param>
    /// <returns></returns>
    Task AddOrUpdateControlImage(ControlImageMetaData imageMetaData, byte[] fileData);

    /// <summary>
    /// Delete the image from the database
    /// </summary>
    /// <param name="control">ControlImageMetaData that can be used to identify the image to be deleted</param>
    /// <returns></returns>
    Task DeleteImageAsync(ControlImageMetaData control);
}
