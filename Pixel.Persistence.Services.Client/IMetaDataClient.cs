using System.Collections.Generic;
using System.Threading.Tasks;
using Pixel.Persistence.Core.Models;

namespace Pixel.Persistence.Services.Client
{
    public interface IMetaDataClient
    {
        Task<IEnumerable<ApplicationMetaData>> GetApplicationMetaData();
        Task<IEnumerable<ProjectMetaData>> GetProjectsMetaData();
        Task<IEnumerable<ProjectMetaData>> GetProjectMetaData(string projectId);
    }
}