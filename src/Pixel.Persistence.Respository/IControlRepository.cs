using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public interface IControlRepository
    {
        IAsyncEnumerable<ControlMetaData> GetMetadataAsync(string applicationId);

        IAsyncEnumerable<DataFile> GetControlFiles(string applicationId, string controlId);
   
        Task AddOrUpdateControl(ControlMetaData application, string fileName, byte[] fileData);

        Task AddOrUpdateControlImage(ControlImageMetaData application, string fileName, byte[] fileData);
    }
}
