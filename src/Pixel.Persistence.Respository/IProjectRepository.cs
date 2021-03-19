using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public interface IProjectRepository
    {
        Task<byte[]> GetProjectFile(string projectId);

        Task<byte[]> GetProjectDataFiles(string projectId, string version);

        IAsyncEnumerable<ProjectMetaData> GetProjectsMetadataAsync();

        IAsyncEnumerable<ProjectMetaData> GetProjectMetadataAsync(string projectId);

        Task AddOrUpdateProject(ProjectMetaData application, string fileName, byte[] fileData);
    }
}
