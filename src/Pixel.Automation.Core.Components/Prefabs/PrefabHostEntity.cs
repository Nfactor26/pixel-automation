using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

    [NonSerialized]
    private Entity designTimePrefabEntity;

    [DataMember(Order = 510)]
    [Browsable(false)]
    public string ApplicationId { get; set; }

    [DataMember(Order = 520)]
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
            if(!isInDesignMode)
            {
                this.designTimePrefabEntity = null;
            }
            OnPropertyChanged();
        }
    }

    public PrefabHostEntity() : base("Prefab Host", "PrefabHost")
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
        var prefabLoader = this.EntityManager.GetServiceOfType<IPrefabLoader>();
        this.prefabEntity = await prefabLoader.GetPrefabEntity(ApplicationId, PrefabId, this.EntityManager);
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
            await base.OnCompletionAsync();
        }
        finally
        {
            this.prefabEntity = null;
            this.Components.Clear();           
        }
        if (this.IsInDesignMode)
        {
            Debug.Assert(this.designTimePrefabEntity is not null, "Design Time PrefabEntity should not be null");
            AddComponent(this.designTimePrefabEntity);
        }
    }

    public override async Task OnFaultAsync(IComponent faultingComponent)
    {
        try
        {
            if (this.prefabEntity is not null)
            {
                var prefabEntityManager = this.prefabEntity.EntityManager;
                prefabEntityManager.Dispose();
            }
            await base.OnFaultAsync(faultingComponent);
        }
        finally
        {
            this.prefabEntity = null;
            this.Components.Clear();
        }
        if (this.IsInDesignMode)
        {
            Debug.Assert(this.designTimePrefabEntity is not null, "Design Time PrefabEntity should not be null");
            AddComponent(this.designTimePrefabEntity);
        }

    }

    #region overridden methods

    public override Entity AddComponent(IComponent component)
    {
        if(this.IsInDesignMode && component is PrefabEntity prefabEntity && !this.Components.Any())
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
