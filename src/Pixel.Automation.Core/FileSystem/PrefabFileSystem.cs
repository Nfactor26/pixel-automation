using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.IO;

namespace Pixel.Automation.Core;

public class PrefabFileSystem : VersionedFileSystem, IPrefabFileSystem
{
    private string applicationId;
    private string prefabId;

    /// <InheritDoc/>      
    public string PrefabFile { get; private set; }

    /// <InheritDoc/> 
    public string ProcessFile { get; private set; }

    /// <InheritDoc/> 
    public string ScriptFile { get; private set;  }

    /// <InheritDoc/> 
    public string DataModelFile { get; private set;  }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="applicationSettings"></param>
    public PrefabFileSystem(ISerializer serializer, ApplicationSettings applicationSettings) : base(serializer, applicationSettings)
    {

    }

    /// <InheritDoc/>      
    public void Initialize(Prefab prefab, VersionInfo version)
    {
        Guard.Argument(prefab).NotNull();
        this.Initialize(prefab.ApplicationId, prefab.PrefabId, version);
    }
   
    /// <InheritDoc/>      
    public override void SwitchToVersion(VersionInfo version)
    {
        Initialize(this.applicationId, this.prefabId, version);
    }

    /// <InheritDoc/>      
    public void Initialize(string applicationId, string prefabId, VersionInfo version)
    {
        this.applicationId = Guard.Argument(applicationId).NotNull().NotEmpty();
        this.prefabId = Guard.Argument(prefabId).NotNull().NotEmpty();
        this.ActiveVersion = Guard.Argument(version).NotNull();

        this.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId, Constants.PrefabsDirectory, prefabId, version.ToString());

        base.Initialize();

        this.PrefabFile = Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId, Constants.PrefabsDirectory, prefabId, $"{prefabId}.prefab");
        this.ProcessFile = Path.Combine(this.WorkingDirectory, $"{prefabId}.proc");
        this.ScriptFile = Path.Combine(this.ScriptsDirectory, "InitializePrefab.csx");
        this.DataModelFile = Path.Combine(this.DataModelDirectory, "DataModel.cs");
       
    }

    /// <InheritDoc/>      
    public Entity GetPrefabEntity()
    {
        if (!File.Exists(this.ProcessFile))
        {
            throw new FileNotFoundException($"{this.PrefabFile} not found");
        }
        Entity prefabRoot = this.serializer.Deserialize<Entity>(ProcessFile);
        return prefabRoot;
    }    

}
