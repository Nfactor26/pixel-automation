using Pixel.Automation.Core.Models;
using System;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IPrefabLoader
    {
        Entity LoadPrefab(string applicationId, string prefabId, PrefabVersion prefabVersion, EntityManager primaryEntityManager);
    }
}
