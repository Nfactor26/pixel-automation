using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client.Interfaces;

/// <summary>
/// Interface for managing Prefabs as independent entities
/// </summary>
public interface IPrefabsRepositoryClientV2
{
    /// <summary>
    /// Get Prefab by Id for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="prefabId"></param>
    /// <returns></returns>
    Task<Prefab> GetByIdAsync(string projectId, string projectVersion, string prefabId);

    /// <summary>
    /// Get Prefab by name for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="displayName"></param>
    /// <returns></returns>
    Task<TestCase> GetByNameAsync(string projectId, string projectVersion, string displayName);

    /// <summary>
    /// Get all the Prefabs for a given version of project which were modified since specified time
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="laterThan"></param>
    /// <returns></returns>
    Task<IEnumerable<Prefab>> GetAllForProjectAsync(string projectId, string projectVersion, DateTime laterThan);

    /// <summary>
    /// Add a new Prefab to a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="prefab"></param>
    /// <returns></returns>
    Task<Prefab> AddPrefabAsync(string projectId, string projectVersion, Prefab prefab);

    /// <summary>
    /// Update an existing Prefab for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="prefab"></param>
    /// <returns></returns>
    Task<Prefab> UpdatePrefabAsync(string projectId, string projectVersion, Prefab prefab);

    /// <summary>
    /// Delete an existing Prefab for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="prefabId"></param>
    /// <returns></returns>
    Task DeletePrefabAsync(string projectId, string projectVersion, string prefabId);
}
