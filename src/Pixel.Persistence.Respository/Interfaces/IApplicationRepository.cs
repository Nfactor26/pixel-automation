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
    /// Add or update the ApplicationDescription data 
    /// </summary>
    /// <param name="applicationDescriptionJson">json serialized application description </param>
    /// <returns></returns>
    Task AddOrUpdate(string applicationDescriptionJson);

    /// <summary>
    /// Mark application as deleted
    /// </summary>
    /// <param name="applicationId"></param>
    /// <returns></returns>
    Task DeleteAsync(string applicationId);

}
