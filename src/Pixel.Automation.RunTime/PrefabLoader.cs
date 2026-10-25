using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Reference.Manager;
using Pixel.Automation.Reference.Manager.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pixel.Automation.RunTime
{
    /// <summary>
    /// PrefabLoader is responsible for loading and configuring a prefab process.
    /// This is registered in Thread Scope with DI container to enable parallel execution of same Prefab process.
    /// </summary>
    public class PrefabLoader : IPrefabLoader, IDisposable
    {
        protected readonly ILogger logger = Log.ForContext<PrefabLoader>();
        protected readonly IProjectFileSystem projectFileSystem;
        protected readonly IReferenceManager referenceManager;
        protected readonly Dictionary<string, PrefabInstance> Prefabs = new();
        
        /// <summary>
        /// /constructor
        /// </summary>
        /// <param name="projectFileSystem"></param>
        /// <param name="referenceManager"></param>
        public PrefabLoader(IProjectFileSystem projectFileSystem, IReferenceManager referenceManager)
        {
            this.projectFileSystem = Guard.Argument(projectFileSystem, nameof(projectFileSystem)).NotNull().Value;
            this.referenceManager = Guard.Argument(referenceManager, nameof(referenceManager)).NotNull().Value;
            logger.Debug($"Created a new instance of {nameof(PrefabLoader)} for Thread with Id : {Thread.CurrentThread.ManagedThreadId}");
        }

        /// <inhertidoc/>
        public Entity GetPrefabEntity(string applicationId, string prefabId, IEntityManager parentEntityManager)
        {
            try
            {

                logger.Information($"{nameof(GetPrefabEntity)} request received for prefab with applicationId {applicationId} && prefabId : {prefabId}");
                if (Prefabs.ContainsKey(prefabId))
                {
                    logger.Information($"Prefab with applicationId {applicationId} && prefabId : {prefabId} is available in cache.");
                    var existingPrefabInstance = Prefabs[prefabId];
                    return existingPrefabInstance.GetPrefabRootEntity();
                }

                var prefabInstance = LoadPrefab(applicationId, prefabId, parentEntityManager);
                Prefabs.Add(prefabId, prefabInstance);
                return prefabInstance.GetPrefabRootEntity();
            }
            finally
            {
                logger.Information($"{nameof(GetPrefabEntity)} request completed for prefab with applicationId {applicationId} && prefabId : {prefabId}");
            }
        }

        /// <inhertidoc/>
        public bool IsPrefabLoaded(string prefabId)
        {
            return this.Prefabs.ContainsKey(prefabId);
        }

        /// <inhertidoc/>
        public VersionInfo GetPrefabVersion(string prefabId)
        {
            if (this.Prefabs.ContainsKey(prefabId))
            {
                return this.Prefabs[prefabId].Version;
            }
            throw new ArgumentException("Prefab : '{0}' is not loaded");
        }

        /// <inhertidoc/>
        public void UnloadAndDispose(string prefabId)
        {
            if (this.IsPrefabLoaded(prefabId))
            {
                var prefab = this.Prefabs[prefabId];
                prefab.Dispose();
                this.Prefabs.Remove(prefabId);
                logger.Information("Version : '{0}' of Prefab : '{1}' was unloaded", prefabId, prefab.Version);
            }
        }


        private PrefabInstance LoadPrefab(string applicationId, string prefabId, IEntityManager parentEntityManager)
        {
            Guard.Argument(applicationId).NotEmpty().NotNull();
            Guard.Argument(prefabId).NotEmpty().NotNull();
            Guard.Argument(parentEntityManager).NotNull();

            var prefabProject = new PrefabProject() { ApplicationId = applicationId, ProjectId = prefabId };
            var prefabReferences = GetPrefabReferences();
            VersionInfo versionToLoad = prefabReferences.GetPrefabVersionInUse(prefabProject);

            IEntityManager prefabEntityManager = new EntityManager(parentEntityManager);
            prefabEntityManager.SetIdentifier($"Prefab - {prefabId}");
            prefabEntityManager.Arguments = parentEntityManager.Arguments;

            IPrefabFileSystem prefabFileSystem = prefabEntityManager.GetServiceOfType<IPrefabFileSystem>();
            prefabFileSystem.Initialize(prefabProject, versionToLoad);
            prefabEntityManager.SetCurrentFileSystem(prefabFileSystem);        
         

            ConfigureServices(parentEntityManager, prefabFileSystem);

            PrefabInstance prefabInstance = new PrefabInstance(applicationId, prefabId, versionToLoad, prefabEntityManager, prefabFileSystem);
            return prefabInstance;
        }

        protected virtual void ConfigureServices(IEntityManager parentEntityManager, IPrefabFileSystem prefabFileSystem)
        {
            //Process entity manager should be able to resolve any assembly from prefab references folder such as prefab data model assembly 
            var scriptEngineFactory = parentEntityManager.GetServiceOfType<IScriptEngineFactory>();
            scriptEngineFactory.WithAdditionalSearchPaths(prefabFileSystem.ReferencesDirectory);         
        }

        protected virtual PrefabReferences GetPrefabReferences()
        {
            return this.referenceManager.GetPrefabReferences();
        }
              
        /// <inhertidoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {
                foreach (var prefab in this.Prefabs)
                {
                    prefab.Value.Dispose();
                }
                this.Prefabs.Clear();
            }
            logger.Information("Prefab loader was disposed");
        }      
    }
    
}
