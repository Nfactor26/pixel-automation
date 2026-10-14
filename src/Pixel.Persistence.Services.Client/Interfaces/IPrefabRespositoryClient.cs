using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client.Interfaces;

public interface IPrefabRepositoryClient
{

    /// <summary>
    /// Get all the prefabs modified since specified time for a given application
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="laterThan"></param>
    /// <returns></returns>
    Task<IEnumerable<Prefab>> GetPrefabs(string applicationId, DateTime laterThan);

    /// <summary>
    /// Get a prefab project by Id
    /// </summary>
    /// <param name="prefabId"></param>
    /// <returns></returns>
    Task<Prefab> GetByIdAsync(string prefabId, string version); 

    /// <summary>
    /// Add an PrefabProject
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="screenId"></param>
    /// <returns></returns>
    Task<Prefab> AddPrefabToScreenAsync(Prefab prefab, string screenId);
   
    /// <summary>
    /// Update version details for prefab project
    /// </summary>
    /// <param name="prefabId"></param>
    /// <param name="prefabVersion"></param>
    /// <returns></returns>
    Task<Prefab> UpdatePrefabAsync(Prefab prefab);    

    /// <summary>
    /// Mark prefab project as deleted
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <returns></returns>
    Task DeletePrefabAsync(Prefab prefabProject);
}
