using Pixel.Automation.Core.Models;
using System;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IPrefabLoader
    {
        /// <summary>
        /// Get the configured entity for prefab process
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="prefabId"></param>
        /// <param name="prefabVersion"></param>
        /// <param name="primaryEntityManager"></param>
        /// <returns></returns>
        Entity GetPrefabEntity(string applicationId, string prefabId, IEntityManager primaryEntityManager);

        /// <summary>
        /// Get the data model type for prefab process
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="prefabId"></param>
        /// <param name="primaryEntityManager"></param>
        /// <returns></returns>
        Type GetPrefabDataModelType(string applicationId, string prefabId, IEntityManager primaryEntityManager);

        /// <summary>
        /// Get the loaded version of prefab
        /// </summary>     
        /// <param name="prefabId"></param>
        /// <returns></returns>
        VersionInfo GetPrefabVersion(string prefabId);

        /// <summary>
        /// Check if specified version of prefab is already loaded by PrefabLoader
        /// </summary>
        /// <param name="applicationId"></param>      
        /// <returns></returns>
        bool IsPrefabLoaded(string prefabId);

        /// <summary>
        /// Unload the specified veresion of prefab and dispose it
        /// </summary>
        /// <param name="applicationId"></param>      
        void UnloadAndDispose(string prefabId);
               
    }
}
