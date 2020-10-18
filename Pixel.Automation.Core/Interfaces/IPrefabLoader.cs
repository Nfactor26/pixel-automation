using Pixel.Automation.Core.Models;
using System;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IPrefabLoader
    {
        /// <summary>
        /// Load a prefab process and initialize it's EntityManager and set the data model
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="prefabId"></param>
        /// <param name="prefabVersion"></param>
        /// <param name="primaryEntityManager"></param>
        /// <returns></returns>
        Entity LoadPrefab(string applicationId, string prefabId, PrefabVersion prefabVersion, IEntityManager primaryEntityManager);

    }
}
