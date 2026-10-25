using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Components.Prefabs;

[DataContract]
[Serializable]
public class PrefabEntity : Entity
{
    private readonly ILogger logger = Log.ForContext<PrefabEntity>();
    [DataMember(Order = 500)]
    [Browsable(false)]
    public string ApplicationId { get; set; }

    [DataMember(Order = 510)]
    [Browsable(false)]    
    public string PrefabId { get; set; }

    [NonSerialized]
    private IPrefabLoader prefabLoader;

    [NonSerialized]
    private Entity prefabEntity;

    [IgnoreDataMember]
    [Browsable(false)]
    public override List<IComponent> Components
    {
        get => base.Components;            
    }
           
    public PrefabEntity() : base("Prefab Entity", "PrefabEntity")
    {

    }

    public override async Task BeforeProcessAsync()
    {
        try
        {
            Debug.Assert(!this.Components.Any(), "There should be no child componets in a Prefab Entity");
            await LoadPrefab();
            base.AddComponent(this.prefabEntity);         
        }
        catch
        {
            this.Components.Clear();
            throw;
        }
    }

    public async Task LoadPrefab()
    {
        if(this.prefabLoader == null)
        {
            this.prefabLoader = this.EntityManager.GetServiceOfType<IPrefabLoader>();              
        }           
        this.prefabEntity = prefabLoader.GetPrefabEntity(this.ApplicationId, this.PrefabId, this.EntityManager);       
        var fileSystem = this.prefabEntity.EntityManager.GetCurrentFileSystem();
        var scriptFile = Path.Combine(fileSystem.ScriptsDirectory, Constants.InitializeEnvironmentScript);
        var scriptEngine = this.prefabEntity.EntityManager.GetScriptEngine();
        scriptEngine.ClearState();
        await scriptEngine.ExecuteFileAsync(scriptFile);
        logger.Information($"Executed initialization script : '{0}' for prefab : '{1}'", scriptFile, this.PrefabId);
    }

    public override async Task OnCompletionAsync()
    {
        this.Components.Clear();
    }

    public override async Task OnFaultAsync(IComponent faultingComponent)
    {
        this.Components.Clear();       
        await base.OnFaultAsync(faultingComponent);
    }

    #region overridden methods

    public override Entity AddComponent(IComponent component)
    {           
        return this;
    }

    #endregion overridden methods
}
