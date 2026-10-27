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
public class PrefabHostEntity : Entity
{
    private readonly ILogger logger = Log.ForContext<PrefabHostEntity>();
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

    [NonSerialized]
    private Entity designTimePrefabEntity;

    public override List<IComponent> Components => base.Components;

    private bool isInDesignMode;
    [IgnoreDataMember]
    [Browsable(false)]
    public bool IsInDesignMode
    {
        get => isInDesignMode;
        set
        {
            isInDesignMode = value;
            if (!isInDesignMode)
            {
                this.designTimePrefabEntity = null;
            }
            OnPropertyChanged();
        }
    }

    public PrefabHostEntity() : base("Prefab Host Entity", "PrefabHostEntity")
    {

    }

    public override async Task BeforeProcessAsync()
    {
        //When testcase is executed while prefab is in design mode, clear the components so that new 
        //component can be loaded for execution. This is needed since design time and runtime uses different arguments.
        if (this.IsInDesignMode)
        {
            this.Components.Clear();
        }

        try
        {
            Debug.Assert(!this.Components.Any(), "There should be no child componets in a Prefab Host Entity");
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
        scriptEngine.SetWorkingDirectory(fileSystem.WorkingDirectory);
        scriptEngine.ClearState();
        await scriptEngine.ExecuteFileAsync(scriptFile);
        logger.Information($"Executed initialization script : '{0}' for prefab : '{1}'", scriptFile, this.PrefabId);
    }

    public override async Task OnCompletionAsync()
    {
        this.Components.Clear();
        if (this.IsInDesignMode)
        {
            Debug.Assert(this.designTimePrefabEntity is not null, "Design Time PrefabEntity should not be null");
            AddComponent(this.designTimePrefabEntity);
        }
    }

    public override async Task OnFaultAsync(IComponent faultingComponent)
    {
        this.Components.Clear();       
        await base.OnFaultAsync(faultingComponent);
        if (this.IsInDesignMode)
        {
            Debug.Assert(this.designTimePrefabEntity is not null, "Design Time PrefabEntity should not be null");
            AddComponent(this.designTimePrefabEntity);
        }
    }

    #region overridden methods

    public override Entity AddComponent(IComponent component)
    {
        if (this.IsInDesignMode && component is PrefabEntity prefabEntity && !this.Components.Any())
        {
            this.Components.Add(component);          
            prefabEntity.EntityManager.RestoreParentChildRelation(prefabEntity);
            this.designTimePrefabEntity = prefabEntity;
        }
        return this;
    }

    #endregion overridden methods

    public bool ShouldSerializeComponents()
    {
        return false;
    }
}
