using Pixel.Automation.Core.Interfaces;
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
    [NonSerialized]
    private Entity prefabEntity;    

    [DataMember(Order = 510)]
    [Browsable(false)]
    public string PrefabId { get; set; }
   
    public override List<IComponent> Components => base.Components;

    private bool isInDesignMode;
    [IgnoreDataMember]
    [Browsable (false)]
    public bool IsInDesignMode
    {
        get => isInDesignMode;
        set
        {
            isInDesignMode = value;
            OnPropertyChanged();
        }
    }

    public PrefabHostEntity() : base("Prefab Host", "PrefabHost")
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
        var fileSystem = this.EntityManager.GetCurrentFileSystem() as IProjectFileSystem;
        this.prefabEntity = fileSystem.LoadFile<PrefabEntity>(Path.Combine(fileSystem.PrefabsRepository, this.PrefabId, $"{this.PrefabId}.proc"));
        
        IEntityManager prefabEntityManager = new EntityManager(this.EntityManager);
        prefabEntityManager.SetIdentifier($"Prefab - {this.PrefabId}");
        prefabEntity.EntityManager = prefabEntityManager;
        prefabEntityManager.Arguments = this.EntityManager.Arguments;

        var scriptEngine = prefabEntityManager.GetScriptEngine();
        scriptEngine.ClearState();
        await scriptEngine.ExecuteFileAsync(Path.Combine(fileSystem.PrefabsRepository, this.PrefabId, $"{this.PrefabId}.csx"));
    }

    public override async Task OnCompletionAsync()
    {
        try
        {
            if(this.prefabEntity is not null)
            {
                var prefabEntityManager = this.prefabEntity.EntityManager;
                prefabEntityManager.Dispose();
            }
            this.prefabEntity = null;
        }
        finally
        {
          
            this.Components.Clear();           
        }
    }

    public override async Task OnFaultAsync(IComponent faultingComponent)
    {
        if (this.prefabEntity is not null)
        {
            var prefabEntityManager = this.prefabEntity.EntityManager;
            prefabEntityManager.Dispose();
        }
        this.prefabEntity = null;
        this.Components.Clear();    
        await base.OnFaultAsync(faultingComponent);
    }

    #region overridden methods

    public override Entity AddComponent(IComponent component)
    {
        if(component is PrefabEntity prefabEntity && !this.Components.Any())
        {
            this.Components.Add(component);
            prefabEntity.EntityManager.RestoreParentChildRelation(prefabEntity);
        }
        return this;
    }

    #endregion overridden methods


    public bool ShouldSerializeComponents()
    {      
        return false;
    }

}
