﻿using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    /// <summary>
    /// IPrefabRepository is used to manage a Prefab
    /// </summary>
    public interface IPrefabsRepository
    {
        /// <summary>
        /// Get a prefab by Id
        /// </summary>
        /// <param name="prefabId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<PrefabProject> FindByIdAsync(string prefabId, CancellationToken cancellationToken);

        /// <summary>
        /// Get a prefab by name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<PrefabProject> FindByNameAsync(string name, CancellationToken cancellationToken);

        /// <summary>
        /// Get all available prefabs
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<PrefabProject>> FindAllAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Add a new prefab
        /// </summary>
        /// <param name="prefabProject"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task AddPrefabAsync(PrefabProject prefabProject, CancellationToken cancellationToken);

        /// <summary>
        /// Add a new version of a prefab
        /// </summary>
        /// <param name="prefabId"></param>
        /// <param name="newVersion"></param>
        /// <param name="cloneFrom"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task AddPrefabVersionAsync(string prefabId, PrefabVersion newVersion, PrefabVersion cloneFrom, CancellationToken cancellationToken);

        /// <summary>
        /// Update an existing version of prefab
        /// </summary>
        /// <param name="prefabId"></param>
        /// <param name="version"></param>    
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task UpdatePrefabVersionAsync(string prefabId, PrefabVersion version, CancellationToken cancellationToken);
    }
}
