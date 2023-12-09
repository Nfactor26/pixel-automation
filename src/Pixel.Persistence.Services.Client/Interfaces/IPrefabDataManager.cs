using Pixel.Automation.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client.Interfaces;

public interface IPrefabDataManager
{
    /// <summary>
    /// Get all the locally available prefabs for a given application
    /// </summary>
    /// <param name="applicationId"></param>
    /// <returns></returns>
    IEnumerable<PrefabProject> GetAllPrefabs(string applicationId);

    /// <summary>
    /// Get all the prefabs for a given screen of an application
    /// </summary>
    /// <param name="applicationDescription"></param>
    /// <param name="screenName"></param>
    /// <returns></returns>
    IEnumerable<PrefabProject> GetPrefabsForScreen(ApplicationDescription applicationDescription, string screenName);

    /// <summary>
    /// Download all the available Prefabs. This doesn't include prefab data.
    /// </summary>
    /// <returns></returns>
    Task DownloadPrefabsAsync();

    /// <summary>
    /// Download data files for a given version of prefab
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="prefabId"></param>
    /// <param name="prefabVersion"></param>
    /// <returns></returns>
    Task DownloadPrefabDataAsync(string applicationId, string prefabId, string prefabVersion);

    /// <summary>
    /// Download data files for a given version of Prefab
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <param name="prefabVersion"></param>
    /// <returns></returns>
    Task DownloadPrefabDataAsync(PrefabProject prefabProject, PrefabVersion prefabVersion);

    /// <summary>
    /// Download a file with specified nme for the version of AutomtionProject being managed
    /// </summary>
    /// <returns></returns>
    Task DownloadPrefabDataFileByNameAsync(PrefabProject prefabProject, PrefabVersion prefabVersion, string fileName);

    /// <summary>
    /// Download dta model files (*.cs) for a given version of prefab
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <param name="prefabVersion"></param>
    /// <returns></returns>
    Task DownloadDataModelFilesAsync(PrefabProject prefabProject, PrefabVersion prefabVersion);

    /// <summary>
    /// Add a new prefab
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <returns></returns>
    Task AddPrefabAsync(PrefabProject prefabProject);

    /// <summary>
    /// Add a new version to project by cloning data from an existing version
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <param name="newVersion"></param>
    /// <param name="cloneFrom"></param>
    /// <returns></returns>
    Task AddPrefabVersionAsync(PrefabProject prefabProject, PrefabVersion newVersion, PrefabVersion cloneFrom);

    /// <summary>
    /// Update details of an existing version of PrefabProject
    /// </summary>
    /// <param name=""></param>
    /// <param name="prefabVersion"></param>
    /// <returns></returns>
    Task UpdatePrefabVersionAsync(PrefabProject prefabProject, PrefabVersion prefabVersion);

    /// <summary>
    /// Check if a Prefb project is marked deleted.
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <returns></returns>
    Task<bool> CheckIfDeletedAsync(PrefabProject prefabProject);

    /// <summary>
    /// Mark the Prefab project as deleted.
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <returns></returns>
    Task DeletePrefbAsync(PrefabProject prefabProject);

    /// <summary>
    /// Add a data file to a given version of prefab project
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <param name="prefabVersion"></param>
    /// <param name="fileName"></param>
    /// <param name="filePath"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    Task AddOrUpdateDataFileAsync(PrefabProject prefabProject, PrefabVersion prefabVersion, string filePath, string tag);

    /// <summary>
    /// Delete a data file belonging to given version of prefab project
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <param name="prefabVersion"></param>
    /// <param name="fileToDelete"></param>
    /// <returns></returns>
    Task DeleteDataFileAsync(PrefabProject prefabProject, PrefabVersion prefabVersion, string fileToDelete);

    /// <summary>
    /// Save all the data files belonging to a specific version of Prefab
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <param name="prefabVerssion"></param>
    /// <returns></returns>
    Task SavePrefabDataAsync(PrefabProject prefabProject, PrefabVersion prefabVerssion);

}
