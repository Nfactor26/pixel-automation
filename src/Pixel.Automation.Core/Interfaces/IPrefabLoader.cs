using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces;

public interface IPrefabLoader
{
    /// <summary>
    /// Get the configured prefab entity for prefab
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="prefabId"></param>    
    /// <param name="hostEntityManager"></param>
    /// <returns></returns>
    Task<Entity> GetPrefabEntity(string applicationId, string prefabId, IEntityManager hostEntityManager);
   
}
