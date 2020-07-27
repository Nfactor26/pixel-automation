using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public interface IApplicationRepository
    {
        Task<byte[]> GetApplicationFile(string applicationId);

        IAsyncEnumerable<ApplicationMetaData> GetMetadataAsync();

        Task AddOrUpdate(ApplicationMetaData application, string fileName,  byte[] fileData);
     
    }
}
