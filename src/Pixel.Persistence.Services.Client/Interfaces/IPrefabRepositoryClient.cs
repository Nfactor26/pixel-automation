using Pixel.Automation.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface IPrefabRepositoryClient
    {
        Task<string> AddOrUpdatePrefabAsync(PrefabDescription prefabDescription, string prefabDescriptionFile);
        Task<string> AddOrUpdatePrefabDataFilesAsync(PrefabDescription prefabDescription, VersionInfo version, string prefabDescriptionFile);
        Task<byte[]> GetPrefabDataFilesAsync(string prefabId, string version);
        Task<PrefabDescription> GetPrefabFileAsync(string prefabId);
    }

}
