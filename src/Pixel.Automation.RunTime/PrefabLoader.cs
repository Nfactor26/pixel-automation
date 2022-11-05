﻿using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Reference.Manager;
using Pixel.Scripting.Reference.Manager.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        protected readonly Dictionary<string, PrefabInstance> Prefabs = new Dictionary<string, PrefabInstance>();
        private PrefabReferences prefabReferences;
       
        public PrefabLoader(IProjectFileSystem projectFileSystem, IReferenceManager referenceManager)
        {
            this.projectFileSystem = Guard.Argument(projectFileSystem, nameof(projectFileSystem)).NotNull().Value;
            this.referenceManager = Guard.Argument(referenceManager, nameof(referenceManager)).NotNull().Value;
            logger.Debug($"Created a new instance of {nameof(PrefabLoader)} for Thread with Id : {Thread.CurrentThread.ManagedThreadId}");
        }

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

        public Type GetPrefabDataModelType(string applicationId, string prefabId, IEntityManager parentEntityManager)
        {
            try
            {
                logger.Information($"{nameof(GetPrefabDataModelType)} request received for prefab with applicationId {applicationId} && prefabId : {prefabId}");
                if (Prefabs.ContainsKey(prefabId))
                {
                    logger.Information($"Prefab with applicationId {applicationId} && prefabId : {prefabId} is available in cache.");
                    var existingPrefabInstance = Prefabs[prefabId];
                    return existingPrefabInstance.GetDataModelType();
                }

                var prefabInstance = LoadPrefab(applicationId, prefabId, parentEntityManager);
                Prefabs.Add(prefabId, prefabInstance);
                return prefabInstance.GetDataModelType();
            }
            finally
            {
                logger.Information($"{nameof(GetPrefabDataModelType)} request completed for prefab with applicationId {applicationId} && prefabId : {prefabId}");
            }
        }

        public void ClearCache()
        {
            foreach(var prefab in this.Prefabs)
            {
                prefab.Value.Dispose();
            }
            this.Prefabs.Clear();
            logger.Information("Prefab loader cached was cleared");
        }

        private PrefabInstance LoadPrefab(string applicationId, string prefabId, IEntityManager parentEntityManager)
        {
            Guard.Argument(applicationId).NotEmpty().NotNull();
            Guard.Argument(prefabId).NotEmpty().NotNull();
            Guard.Argument(parentEntityManager).NotNull();

            var prefabProject = new PrefabProject() { ApplicationId = applicationId, PrefabId = prefabId };
            var prefabReferences = GetPrefabReferences();
            PrefabVersion versionToLoad = prefabReferences.GetPrefabVersionInUse(prefabProject);

            IEntityManager prefabEntityManager = new EntityManager(parentEntityManager);
            prefabEntityManager.SetIdentifier($"Prefab - {prefabId}");
            IPrefabFileSystem prefabFileSystem = prefabEntityManager.GetServiceOfType<IPrefabFileSystem>();
            prefabFileSystem.Initialize(prefabProject, versionToLoad);
            prefabEntityManager.SetCurrentFileSystem(prefabFileSystem);    
         
            string prefabAssembly = Path.Combine(prefabFileSystem.ReferencesDirectory, versionToLoad.DataModelAssembly);
            if (!prefabFileSystem.Exists(prefabAssembly))
            {
                throw new FileNotFoundException($"Prefab data model assembly : {prefabAssembly} couldn't be located");
            }
            Assembly prefabDataModelAssembly = Assembly.LoadFrom(prefabAssembly);

            Type dataModelType = prefabDataModelAssembly.GetTypes().FirstOrDefault(t => t.Name.Equals(Constants.PrefabDataModelName));
            if (dataModelType == null)
            {
                throw new NullReferenceException($"Failed to find type {Constants.PrefabDataModelName} in prefab data model assembly for prefab with applicationId : {applicationId}" +
                    $"and preafbId : {prefabId}");
            }

            ConfigureServices(parentEntityManager, prefabFileSystem, prefabDataModelAssembly);

            PrefabInstance prefabInstance = new PrefabInstance(prefabEntityManager, prefabFileSystem, dataModelType);
            return prefabInstance;
        }

        protected virtual void ConfigureServices(IEntityManager parentEntityManager, IPrefabFileSystem prefabFileSystem, Assembly prefabAssembly)
        {
            //Process entity manager should be able to resolve any assembly from prefab references folder such as prefab data model assembly 
            var scriptEngineFactory = parentEntityManager.GetServiceOfType<IScriptEngineFactory>();
            scriptEngineFactory.WithAdditionalSearchPaths(prefabFileSystem.ReferencesDirectory);
            scriptEngineFactory.WithAdditionalAssemblyReferences(prefabAssembly);
          
        }

        protected virtual PrefabReferences GetPrefabReferences()
        {
            return this.referenceManager.GetPrefabReferences();
        }


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
