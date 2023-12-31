using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository;

/// <summary>
/// IApplicationRepository is used to manage the ApplicationDescription data stored in database
/// </summary>
public interface IApplicationRepository
{
    /// <summary>
    /// Get the application data document for a given applicationId
    /// </summary>
    /// <param name="applicationId"></param> 
    /// <returns>Contents of application file</returns>
    Task<object> FindByIdAsync(string applicationId);

    /// <summary>
    /// Get all applications for a given platform that were modified since specified time
    /// </summary>
    /// <param name="laterThan"></param>
    /// <returns></returns>
    Task<IEnumerable<object>> GetAllApplications(string platform, DateTime laterThan);

    /// <summary>
    /// Add a new application 
    /// </summary>
    /// <param name="application"></param>    
    /// <returns></returns>
    Task AddApplication(object applicationDescription);

    /// <summary>
    /// Update an existing application
    /// </summary>
    /// <param name="application"></param> 
    /// <returns></returns>
    Task UpdateApplication(object applicationDescription);

    /// <summary>
    /// Mark application as deleted
    /// </summary>
    /// <param name="applicationId"></param>
    /// <returns></returns>
    Task DeleteAsync(string applicationId);

    /// <summary>
    /// Add a new screen to the application
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="applicationScreen"></param>
    /// <returns></returns>
    Task AddScreenAsync(string applicationId, ApplicationScreen applicationScreen);

    /// <summary>
    /// Rename an existing screen of the application
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="screenId"></param>
    /// <param name="newScreenName"></param>
    /// <returns></returns>
    Task RenameScreenAsync(string applicationId, string screenId, string newScreenName);    

    /// <summary>
    /// Add an entry for control to specified application screen
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="controlId"></param>
    /// <param name="screenId"></param>
    /// <returns></returns>
    Task AddControlToScreen(string applicationId,  string controlId, string screenId);

    /// <summary>
    /// Remove entry for control from specified application screen
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="controlId"></param>
    /// <param name="screenId"></param>
    /// <param name=""></param>
    /// <returns></returns>
    Task DeleteControlFromScreen(string applicationId,  string controlId, string screenId);

    /// <summary>
    /// Move control from one screen to another
    /// </summary>
    /// <param name="applicationId"></param>    
    /// <param name="controlId"></param>
    /// <param name="targetScreenId"></param>
    /// <returns></returns>
    Task MoveControlToScreen(string applicationId, string controlId, string targetScreenId);

    /// <summary>
    /// Get the screen id for a control to which it belongs
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="controlId"></param>
    /// <returns></returns>
    Task<string> GetScreenForControl(string applicationId, string controlId);

    /// <summary>
    /// Add an entry for prefab to specified application screen
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="screenId"></param>
    /// <param name="prefabId"></param>  
    /// <returns></returns>
    Task AddPrefabToScreen(string applicationId, string prefabId, string screenId);

    /// <summary>
    /// Remove entry of prefab from specified application screen
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="prefabId"></param>
    /// <param name="screenId"></param>
    /// <returns></returns>
    Task DeletePrefabFromScreen(string applicationId, string prefabId, string screenId);

    /// <summary>
    /// Move Prefab from one screen to another
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="prefabId"></param>
    /// <param name="targetScreenId"></param>
    /// <returns></returns>
    Task MovePrefabToScreen(string applicationId, string prefabId, string targetScreenId);

    /// <summary>
    /// Get the screen id for a prefab to which it belongs
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="prefabId"></param>
    /// <returns></returns>
    Task<string> GetScreenForPrefab(string applicationId, string prefabId);
}
