﻿using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Scripting.Editor.Core.Contracts;
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
    public class PrefabLoader : IPrefabLoader
    {
        private readonly ILogger logger = Log.ForContext<PrefabLoader>();
        private readonly IProjectFileSystem projectFileSystem;
        private readonly Dictionary<string, PrefabInstance> Prefabs = new Dictionary<string, PrefabInstance>();
       
        public PrefabLoader(IProjectFileSystem projectFileSystem)
        {
            this.projectFileSystem = projectFileSystem;
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

        private PrefabInstance LoadPrefab(string applicationId, string prefabId, IEntityManager parentEntityManager)
        {
            Guard.Argument(applicationId).NotEmpty().NotNull();
            Guard.Argument(prefabId).NotEmpty().NotNull();
            Guard.Argument(parentEntityManager).NotNull();


            var prefabReferences = GetPrefabReferences();
            PrefabVersion versionToLoad = prefabReferences.GetPrefabVersionInUse(new PrefabDescription() { ApplicationId = applicationId, PrefabId = prefabId });

            IEntityManager prefabEntityManager = new EntityManager(parentEntityManager);
            IPrefabFileSystem prefabFileSystem = prefabEntityManager.GetServiceOfType<IPrefabFileSystem>();
            prefabFileSystem.Initialize(applicationId, prefabId, versionToLoad);
            prefabEntityManager.SetCurrentFileSystem(prefabFileSystem);

            ConfigureServices(parentEntityManager, prefabFileSystem);    

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

            PrefabInstance prefabInstance = new PrefabInstance(parentEntityManager, prefabEntityManager, prefabFileSystem, dataModelType);
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
          //We load this each time since this can change at design time
          return  this.projectFileSystem.LoadFile<PrefabReferences>(this.projectFileSystem.PrefabReferencesFile);
        }
    }

    /// <summary>
    /// DesignTimePrefabLoader configure some additional services such as ScriptEditor while loading a Prefab at design time.
    /// </summary>
    public class DesignTimePrefabLoader : PrefabLoader
    {
        public DesignTimePrefabLoader(IProjectFileSystem projectFileSystem) : base(projectFileSystem)
        {
          
        }

        protected override void ConfigureServices(IEntityManager parentEntityManager, IPrefabFileSystem prefabFileSystem)
        {
            //Process entity manager should be able to resolve any assembly from prefab references folder such as prefab data model assembly 
            var scriptEngineFactory = parentEntityManager.GetServiceOfType<IScriptEngineFactory>();
            scriptEngineFactory.WithAdditionalSearchPaths(prefabFileSystem.ReferencesDirectory);

            //Similalry, Script editor when working with prefab input and output mapping script should be able to resolve references from prefab references folder
            var scriptEditorFactory = parentEntityManager.GetServiceOfType<IScriptEditorFactory>();
            scriptEditorFactory.AddSearchPaths(prefabFileSystem.ReferencesDirectory);
        }
    }

}