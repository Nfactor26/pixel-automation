using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Reference.Manager;
using Pixel.Automation.Reference.Manager.Contracts;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.RunTime;

/// <summary>
/// PrefabLoader is responsible for loading and configuring a prefab process.
/// This is registered in Thread Scope with DI container to enable parallel execution of same Prefab process.
/// </summary>
public class PrefabLoader : IPrefabLoader
{
    protected readonly ILogger logger = Log.ForContext<PrefabLoader>();
    protected readonly ISerializer serializer;
    protected readonly IReferenceManager referenceManager;
    protected readonly ApplicationSettings applicationSettings;
    
    /// <summary>
    /// /constructor
    /// </summary>
    /// <param name="applicationFileSystem"></param>
    /// <param name="referenceManager"></param>
    public PrefabLoader(ApplicationSettings applicationSettings, ISerializer serializer, IReferenceManager referenceManager)
    {
        this.applicationSettings =  Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
        this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
        this.referenceManager = Guard.Argument(referenceManager, nameof(referenceManager)).NotNull().Value;
        logger.Debug($"Created a new instance of {nameof(PrefabLoader)}");
    }

    /// <inhertidoc/>
    public async Task<Entity> GetPrefabEntity(string applicationId, string prefabId, IEntityManager hostEntityManager)
    {
        Guard.Argument(applicationId).NotEmpty().NotNull();
        Guard.Argument(prefabId).NotEmpty().NotNull();
        Guard.Argument(hostEntityManager).NotNull();
       
        Version versionToLoad = GetPrefabVersionInUse(applicationId, prefabId);

        IEntityManager prefabEntityManager = new EntityManager(hostEntityManager);
        prefabEntityManager.SetIdentifier($"Prefab - {prefabId}");
       
        var fileSystem = new PrefabFileSystem(serializer, applicationSettings);
        fileSystem.Initialize(new Prefab() { ApplicationId = applicationId, PrefabId = prefabId }, new VersionInfo(versionToLoad));
        prefabEntityManager.SetCurrentFileSystem(fileSystem);
       
        var prefabEntity = fileSystem.LoadFile<Entity>(fileSystem.ProcessFile);
        prefabEntity.EntityManager = prefabEntityManager;

        prefabEntityManager.RootEntity = prefabEntity;
        prefabEntityManager.Arguments = hostEntityManager.Arguments;

        var scriptEngine = prefabEntityManager.GetScriptEngine();       
        scriptEngine.ClearState();
        scriptEngine.SetWorkingDirectory(fileSystem.WorkingDirectory);
        await scriptEngine.ExecuteFileAsync(fileSystem.ScriptFile);

        return prefabEntity;
    }

    public Version GetPrefabVersionInUse(string applicationId, string prefabId)
    {
        Guard.Argument(applicationId).NotEmpty().NotNull();
        Guard.Argument(prefabId).NotEmpty().NotNull();

        var prefab = new Prefab() { ApplicationId = applicationId, PrefabId = prefabId };
        var prefabReferences = GetPrefabReferences();
        return prefabReferences.GetPrefabVersionInUse(prefab);
        
    }

    protected virtual PrefabReferences GetPrefabReferences()
    {
        return this.referenceManager.GetPrefabReferences();
    } 
 
}

