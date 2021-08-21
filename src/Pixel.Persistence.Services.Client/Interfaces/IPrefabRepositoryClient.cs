using Pixel.Automation.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface IPrefabRepositoryClient
    {
        Task AddOrUpdatePrefabAsync(PrefabProject prefabProject, string prefabFile);
        Task AddOrUpdatePrefabDataFilesAsync(PrefabProject prefabProject, VersionInfo version, string prefabFile);
        Task<byte[]> GetPrefabDataFilesAsync(string prefabId, string version);
        Task<PrefabProject> GetPrefabFileAsync(string prefabId);
    }

}
