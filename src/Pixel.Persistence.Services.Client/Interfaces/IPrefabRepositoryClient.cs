using Pixel.Automation.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface IPrefabRepositoryClient
    {
        Task AddOrUpdatePrefabAsync(PrefabDescription prefabDescription, string prefabDescriptionFile);
        Task AddOrUpdatePrefabDataFilesAsync(PrefabDescription prefabDescription, VersionInfo version, string prefabDescriptionFile);
        Task<byte[]> GetPrefabDataFilesAsync(string prefabId, string version);
        Task<PrefabDescription> GetPrefabFileAsync(string prefabId);
    }

}
