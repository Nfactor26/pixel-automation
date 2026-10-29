using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pixel.Automation.RunTime;

/// <summary>
/// PrefabLoader is responsible for loading and configuring a prefab process.
/// This is registered in Thread Scope with DI container to enable parallel execution of same Prefab process.
/// </summary>
public class PrefabLoader : IPrefabLoader, IDisposable
{
    protected readonly ILogger logger = Log.ForContext<PrefabLoader>();
    protected readonly IProjectFileSystem projectFileSystem;
    protected readonly IReferenceProviderFactory referenceProviderFactory;
    protected readonly Dictionary<string, PrefabInstance> Prefabs = new();
    
    /// <summary>
    /// /constructor
    /// </summary>
    /// <param name="projectFileSystem"></param>
    /// <param name="referenceManager"></param>
    public PrefabLoader(IProjectFileSystem projectFileSystem, IReferenceProviderFactory referenceProviderFactory)
    {
        this.projectFileSystem = Guard.Argument(projectFileSystem, nameof(projectFileSystem)).NotNull().Value;
        this.referenceProviderFactory = Guard.Argument(referenceProviderFactory, nameof(referenceProviderFactory)).NotNull().Value;
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


    private PrefabInstance LoadPrefab(string applicationId, string prefabId, IEntityManager hostEntityManager)
    {
        Guard.Argument(applicationId).NotEmpty().NotNull();
        Guard.Argument(prefabId).NotEmpty().NotNull();
        Guard.Argument(hostEntityManager).NotNull();

        var prefabProject = new PrefabProject() { ApplicationId = applicationId, ProjectId = prefabId };
        var versionToLoad = GetPrefabVersion(prefabProject, hostEntityManager);
      
        IEntityManager prefabEntityManager = new EntityManager(hostEntityManager);
        prefabEntityManager.SetIdentifier($"Prefab - {prefabId}");
        prefabEntityManager.Arguments = hostEntityManager.Arguments;

        IPrefabFileSystem prefabFileSystem = prefabEntityManager.GetServiceOfType<IPrefabFileSystem>();
        prefabFileSystem.Initialize(prefabProject, versionToLoad);
        prefabEntityManager.SetCurrentFileSystem(prefabFileSystem);

        var referenceProvider = this.referenceProviderFactory.CreateReferenceProvider(prefabFileSystem);
        prefabEntityManager.SetRefereceProvider(referenceProvider);

        ConfigureServices(hostEntityManager, prefabFileSystem);

        PrefabInstance prefabInstance = new PrefabInstance(applicationId, prefabId, versionToLoad, prefabEntityManager, prefabFileSystem);
        return prefabInstance;
    }

    protected virtual void ConfigureServices(IEntityManager parentEntityManager, IPrefabFileSystem prefabFileSystem)
    {
        //Script engine should be able to resolve any assembly from prefab references folder 
        var scriptEngineFactory = parentEntityManager.GetServiceOfType<IScriptEngineFactory>();
        scriptEngineFactory.WithAdditionalSearchPaths(prefabFileSystem.ReferencesDirectory);         
    }

    protected virtual VersionInfo GetPrefabVersion(PrefabProject project, IEntityManager hostEntityManager)
    {
        var hostReferenceProvider = hostEntityManager.GetReferenceProvider();
        return hostReferenceProvider.GetPrefabVersion(project.ApplicationId, project.ProjectId);            
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

