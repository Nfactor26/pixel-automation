using Dawn;
using Pixel.Automation.Core.Interfaces;
using System;

namespace Pixel.Automation.Core.Components.Prefabs
{
    /// <summary>
    /// PrefabInstance is a fully configured ready to use instance of a Prefab process.
    /// </summary>
    public class PrefabInstance
    {
        private readonly IEntityManager parentEntityManager;
        private readonly IEntityManager prefabEntityManager;
        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly Type dataModelType;

        public PrefabInstance(IEntityManager parentEntityManager, IEntityManager prefabEntityManager, IPrefabFileSystem prefabFileSystem, Type dataModelType)
        {
            this.parentEntityManager = Guard.Argument(parentEntityManager).NotNull().Value;
            this.prefabEntityManager = Guard.Argument(prefabEntityManager).NotNull().Value;
            this.prefabFileSystem = Guard.Argument(prefabFileSystem).NotNull().Value;
            this.dataModelType = Guard.Argument(dataModelType).NotNull().Value;
        }

        /// <summary>
        /// Get the root entity for prefab process which has EntityManager an data model already setup.
        /// </summary>
        /// <returns></returns>
        public Entity GetPrefabRootEntity()
        {          
            var prefabRoot = this.prefabFileSystem.GetPrefabEntity(this.dataModelType.GetType().Assembly);
            prefabRoot.EntityManager = this.prefabEntityManager;
            this.prefabEntityManager.RestoreParentChildRelation(prefabRoot);
            //Setting argument will initialize all the required services such as script engine, argument processor , etc.
            var dataModelInstance = Activator.CreateInstance(dataModelType);
            this.prefabEntityManager.Arguments = dataModelInstance;
            return prefabRoot;
        }

        /// <summary>
        /// Get the data model type for prefab process
        /// </summary>
        /// <returns></returns>
        public Type GetDataModelType()
        {
            return this.dataModelType;
        }
    }
}
