﻿using Pixel.Automation.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client.Interfaces;

public interface IPrefabsRepositoryClient
{
    /// <summary>
    /// Get a prefab project by Id
    /// </summary>
    /// <param name="prefabId"></param>
    /// <returns></returns>
    Task<PrefabProject> GetByIdAsync(string prefabId);

    /// <summary>
    /// Get an prefab project by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<PrefabProject> GetByNameAsync(string name);

    /// <summary>
    /// Get all the available AutomationProjects
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<PrefabProject>> GetAllAsync();

    /// <summary>
    /// Add an PrefabProject
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <param name="screenId"></param>
    /// <returns></returns>
    Task<PrefabProject> AddPrefabToScreenAsync(PrefabProject prefabProject, string screenId);

    /// <summary>
    /// Add a new version to prefab project by clonining data from another specified version
    /// </summary>
    /// <param name="prefabId"></param>
    /// <param name="newVersion"></param>
    /// <param name="cloneFrom"></param>
    /// <returns></returns>
    Task<VersionInfo> AddPrefabVersionAsync(string prefabId, VersionInfo newVersion, VersionInfo cloneFrom);

    /// <summary>
    /// Update version details for prefab project
    /// </summary>
    /// <param name="prefabId"></param>
    /// <param name="prefabVersion"></param>
    /// <returns></returns>
    Task<VersionInfo> UpdatePrefabVersionAsync(string prefabId, VersionInfo prefabVersion);

    /// <summary>
    /// Check if prefab project is marked deleted
    /// </summary>
    /// <param name="prefabId"></param>
    /// <returns></returns>
    Task<bool> CheckIfDeletedAsync(string prefabId);

    /// <summary>
    /// Mark prefab project as deleted
    /// </summary>
    /// <param name="prefabProject"></param>
    /// <returns></returns>
    Task DeletePrefabAsync(PrefabProject prefabProject);
}
