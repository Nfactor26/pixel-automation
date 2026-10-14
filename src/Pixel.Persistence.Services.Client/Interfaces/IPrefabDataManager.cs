using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client.Interfaces;

public interface IPrefabDataManager
{

    Task<Prefab> GetPrefabByIdAsync(string applicationId, string prefabId, string version);

    /// <summary>
    /// Get the most recent version of the locally available prefabs for a given application
    /// </summary>
    /// <param name="applicationId"></param>
    /// <returns></returns>
    IEnumerable<Prefab> GetAllPrefabs(string applicationId);


    Task<Entity> GetPrefabEntity(Prefab prefab);

    /// <summary>
    /// Get latest versions of the prefabs for a given screen of an application
    /// </summary>
    /// <param name="applicationDescription"></param>
    /// <param name="screenName"></param>
    /// <returns></returns>
    IEnumerable<Prefab> GetPrefabsForScreen(ApplicationDescription applicationDescription, string screenName);   

    /// <summary>
    /// Download data files for a given version of Prefab
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <param name="prefabVersion"></param>
    /// <returns></returns>
    Task DownloadPrefabDataAsync(Prefab prefab);

    /// <summary>
    /// Download a file with specified nme for the version of AutomtionProject being managed
    /// </summary>
    /// <returns></returns>
    Task DownloadPrefabDataFileByNameAsync(Prefab prefabProject, string fileName);
    
    /// <summary>
    /// Add a new prefab
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="screenId"></param>
    /// <returns></returns>
    Task AddPrefabToScreenAsync(Prefab prefab, string screenId);

    /// <summary>
    /// Move prefab from one screen to another
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="targetScreenId"></param>
    /// <returns></returns>
    Task MovePrefabToScreen(Prefab prefab, string targetScreenId);

    /// <summary>
    /// Update details of an existing version of PrefabProject
    /// </summary>
    /// <param name=""></param>
    /// <param name="prefabVersion"></param>
    /// <returns></returns>
    Task UpdatePrefabAsync(Prefab prefab);

    /// <summary>
    /// Mark the Prefab project as deleted.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    Task DeletePrefabAsync(Prefab prefab);

    /// <summary>
    /// Create a new version of Prefab from an existing version of Prefab
    /// </summary>
    /// <param name="fromPrefab"></param>
    /// <param name="newVersion"></param>
    /// <returns></returns>
    Task<Prefab> CreateRevision(Prefab fromPrefab, Version newVersion);

    ///// <summary>
    ///// Add a new version to project by cloning data from an existing version
    ///// </summary>
    ///// <param name="prefab"></param>
    ///// <param name="newVersion"></param>
    ///// <param name="cloneFrom"></param>
    ///// <returns></returns>
    //Task AddPrefabVersionAsync(Prefab prefabProject, VersionInfo newVersion, VersionInfo cloneFrom);   

    /// <summary>
    /// Add a data file to a given version of prefab project
    /// </summary>
    /// <param name="prefab"></param>   
    /// <param name="filePath"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    Task AddOrUpdateDataFileAsync(Prefab prefab, string filePath, string tag);

    /// <summary>
    /// Delete a data file belonging to given version of prefab project
    /// </summary>
    /// <param name="prefab"></param>    
    /// <param name="fileToDelete"></param>
    /// <returns></returns>
    Task DeleteDataFileAsync(Prefab prefab, string fileToDelete);

    /// <summary>
    /// Save all the data files belonging to the version of Prefab
    /// </summary>
    /// <param name="prefab"></param> 
    /// <returns></returns>
    Task SavePrefabDataAsync(Prefab prefab);

    /// <summary>
    /// Get a PrefabFiles which contains path of all the files belonging to Prefab.
    /// This doesn't however include any additional script files belonging to Prefab workflow.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    PrefabFiles GetPrefabFiles(Prefab prefab);

}