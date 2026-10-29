using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Core.Interfaces;

public interface IPrefabLoader
{
    /// <summary>
    /// Get the configured entity for prefab process
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="prefabId"></param>
    /// <param name="prefabVersion"></param>
    /// <param name="hostEntityManager"></param>
    /// <returns></returns>
    Entity GetPrefabEntity(string applicationId, string prefabId, IEntityManager hostEntityManager);   

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
