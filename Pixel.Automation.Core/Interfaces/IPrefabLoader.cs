using System;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IPrefabLoader
    {
        Entity LoadPrefab(string applicationId, string prefabId, Version prefabVersion, EntityManager primaryEntityManager);
    }
}
