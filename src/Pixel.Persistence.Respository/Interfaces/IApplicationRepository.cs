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
    Task<object> GetApplication(string applicationId);

    /// <summary>
    /// Get all applications that were modified since specified 
    /// </summary>
    /// <param name="laterThan"></param>
    /// <returns></returns>
    Task<IEnumerable<object>> GetAllApplications(DateTime laterThan);
   
    /// <summary>
    /// Add or update the ApplicationDescription data 
    /// </summary>
    /// <param name="applicationDescriptionJson">json serialized application description </param>
    /// <returns></returns>
    Task AddOrUpdate(string applicationDescriptionJson);

}
