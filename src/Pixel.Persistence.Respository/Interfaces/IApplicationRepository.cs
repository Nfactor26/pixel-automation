using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public interface IApplicationRepository
    {
        /// <summary>
        /// Get the application files for a given applicationId
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns>Contents of application file</returns>
        Task<byte[]> GetApplicationFile(string applicationId);

        /// <summary>
        /// Get the ApplicationMetaData. This is used on client side to compare if there are any 
        /// newer files on server
        /// </summary>
        /// <returns>Collection of <see cref="ApplicationMetaData"/></returns>
        IAsyncEnumerable<ApplicationMetaData> GetMetadataAsync();

        /// <summary>
        /// Add or Update ApplicationMetaData and application files
        /// </summary>
        /// <param name="application">ApplicationMetaData for the application</param>
        /// <param name="fileName">File name to store data to</param>
        /// <param name="fileData">Content of the file</param>
        /// <returns></returns>
        Task AddOrUpdate(ApplicationMetaData application, string fileName,  byte[] fileData);
     
    }
}
