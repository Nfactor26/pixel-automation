using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.IO;

namespace Pixel.Automation.Core.Components.Prefabs;

/// <summary>
/// PrefabInstance is a fully configured ready to use instance of a Prefab process.
/// </summary>
public class PrefabInstance : IDisposable
{
    private bool isDisposed = false;
    private readonly IEntityManager prefabEntityManager;
    private readonly IPrefabFileSystem prefabFileSystem;
  
    /// <summary>
    /// Identifier of the Prefab
    /// </summary>
    public  string PrefabId { get; init; }

    /// <summary>
    /// Identifier of the application to which prefab belongs
    /// </summary>
    public string ApplicationId { get; init; }

    /// <summary>
    /// Version of the loaded prefab
    /// </summary>
    public VersionInfo Version { get; init; }
    
    /// <summary>
    /// DataModel type of the loaded  prefab
    /// </summary>
    public Type DataModelType { get; init; }      

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="applicationId"></param>
    /// <param name="prefabId"></param>
    /// <param name="version"></param>
    /// <param name="dataModelType"></param>
    /// <param name="prefabEntityManager"></param>
    /// <param name="prefabFileSystem"></param>
    public PrefabInstance(string applicationId, string prefabId, VersionInfo version, Type dataModelType,
        IEntityManager prefabEntityManager, IPrefabFileSystem prefabFileSystem)
    {
        this.ApplicationId = Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotEmpty();
        this.PrefabId = Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
        this.Version = Guard.Argument(version, nameof(version)).NotNull();
        this.DataModelType = Guard.Argument(dataModelType, nameof(dataModelType)).NotNull().Value;
        this.prefabEntityManager = Guard.Argument(prefabEntityManager, nameof(prefabEntityManager)).NotNull().Value;
        this.prefabFileSystem = Guard.Argument(prefabFileSystem, nameof(prefabFileSystem)).NotNull().Value;      
    }

    /// <summary>
    /// Get the root entity for prefab process which has EntityManager an data model already setup.
    /// </summary>
    /// <returns></returns>
    public Entity GetPrefabRootEntity()
    {
        if(isDisposed)
        {
            throw new ObjectDisposedException($"Version : '{Version}' of Prefab Instance: '{PrefabId}' is already disposed");
        }
        if (!File.Exists(this.prefabFileSystem.PrefabFile))
        {
            throw new FileNotFoundException($"{this.prefabFileSystem.PrefabFile} not found");
        }
        var prefabRoot = this.prefabFileSystem.LoadFile<Entity>(this.prefabFileSystem.PrefabFile);
        prefabRoot.EntityManager = this.prefabEntityManager;
        this.prefabEntityManager.RestoreParentChildRelation(prefabRoot);
        //Setting argument will initialize all the required services such as script engine, argument processor , etc.
        var dataModelInstance = Activator.CreateInstance(DataModelType);
        this.prefabEntityManager.Arguments = dataModelInstance;
        return prefabRoot;
    }

    /// <inheritdoc/>   
    public void Dispose()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool isDisposing)
    {
        if(isDisposing)
        {
            var scriptEngineFactory = this.prefabEntityManager.GetServiceOfType<IScriptEngineFactory>();
            scriptEngineFactory.RemoveReferences(this.DataModelType.Assembly);
            this.prefabEntityManager.Dispose();
        }
    }
}
