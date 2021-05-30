using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public interface IPrefabRepository
    {
        /// <summary>
        /// Add or update Prefab description file or data file for a given version of prefab
        /// </summary>
        /// <param name="prefabMetaData">MetaData for the prefab which includes detaails like prefabId, applicationId, version , etc.</param>
        /// <param name="fileName">Name of the file which contains the data</param>
        /// <param name="fileData">PrefabDescription file or zip file containing all files for a given version</param>
        /// <returns></returns>
        Task AddOrUpdatePrefabAsync(PrefabMetaData prefabMetaData, string fileName, byte[] fileData);

        /// <summary>
        /// Get details of all the prefabs belonging to a given application saved in database
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        IAsyncEnumerable<PrefabMetaDataCompact> GetPrefabsMetadataForApplicationAsync(string applicationId);

        /// <summary>
        /// Get details of a given prefab 
        /// </summary>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        Task<PrefabMetaDataCompact> GetPrefabMetadataForPrefabAsync(string applicationId, string prefabId);

        /// <summary>
        /// Get the PrefabDescription file 
        /// </summary>
        /// <param name="prefabId">PrefabId of the Prefab whose data is required</param>
        /// <returns></returns>
        Task<byte[]> GetPrefabFileAsync(string prefabId);

        /// <summary>
        /// Get the prefab data files for a given version of prefab
        /// </summary>
        /// <param name="prefabId">PrefabId of the Prefab</param>
        /// <param name="version">Version of the Prefab</param>
        /// <returns></returns>
        Task<byte[]> GetPrefabDataFilesAsync(string prefabId, string version);
    }
}
