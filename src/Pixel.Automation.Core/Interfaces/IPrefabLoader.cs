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
        /// Clear the prefab loader cache.     
        /// </summary>
        void ClearCache();

    }
}
