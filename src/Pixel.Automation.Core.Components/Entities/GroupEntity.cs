using Pixel.Automation.Core.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components;

[DataContract]
[Serializable]
[NoDropTarget]
public class GroupEntity : Entity
{
    private ActorComponent groupActorComponent;
    /// <summary>
    /// Actor Component which acts on a set of Entities added to PlaceHolder
    /// </summary>
    [Browsable(false)]
    [DataMember(Order = 200)]
    public ActorComponent GroupActor
    {
        get
        {
            //Make sure that EntityManager is set
            if(groupActorComponent != null)
            {
                groupActorComponent.Parent = this;
                groupActorComponent.EntityManager = this.EntityManager;
            }
            return groupActorComponent;
        }
        set
        {
            groupActorComponent = value;
        }
    }

    private PlaceHolderEntity groupPlaceHolder = new PlaceHolderEntity();
    /// <summary>
    /// Container for Entities that Actor will process as a group
    /// </summary>
    [Browsable(false)]
    public PlaceHolderEntity GroupPlaceHolder
    {
        get
        { 
            //Since we are not serializing this, we need to re-initilize this when accessed
            if(groupPlaceHolder == null && (this.Components?.Any() ?? false))
            {
                groupPlaceHolder = this.Components.FirstOrDefault() as PlaceHolderEntity;
            }
            groupPlaceHolder.EntityManager = this.EntityManager;
            return groupPlaceHolder;
        }
        set
        {
            groupPlaceHolder = value;
        }
    }

    public GroupEntity() : base()
    {

    }

    public override IEnumerable<Core.Interfaces.IComponent> GetNextComponentToProcess()
    {
        yield return this.GroupActor;
        yield break;
    }

    public override void ResolveDependencies()
    {         
        if (this.Components.Count() > 0)
        {
            return;
        }
        base.AddComponent(GroupPlaceHolder);

    }

      
    public override Entity AddComponent(Interfaces.IComponent component)
    {
        return this;
    }
}
