using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
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
        Task<object> GetApplicationData(string applicationId);

        /// <summary>
        /// Get the <see cref="ApplicationMetaData"/>. This is used on client side to compare if there are any 
        /// newer files on server
        /// </summary>
        /// <returns>Collection of <see cref="ApplicationMetaData"/></returns>
        IAsyncEnumerable<ApplicationMetaData> GetMetadataAsync();

        /// <summary>
        /// Add or update the ApplicationDescription data 
        /// </summary>
        /// <param name="applicationDescriptionJson">json serialized application description </param>
        /// <returns></returns>
        Task AddOrUpdate(string applicationDescriptionJson);

    }
}
