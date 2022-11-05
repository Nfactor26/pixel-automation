using System.Collections.Generic;
using System.Threading.Tasks;
using Pixel.Persistence.Core.Models;

namespace Pixel.Persistence.Services.Client
{
    public interface IMetaDataClient
    {
        /// <summary>
        /// Get ApplicationMetaData for most recent saved application data on server
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ApplicationMetaData>> GetApplicationMetaData();    
      
    }
}