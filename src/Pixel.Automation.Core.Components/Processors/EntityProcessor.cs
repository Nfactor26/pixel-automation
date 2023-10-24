using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Components.Processors;

[DataContract]
[Serializable]
public abstract class EntityProcessor : Entity, IEntityProcessor
{
    private readonly ILogger logger = Log.ForContext<EntityProcessor>();
           
    protected int postDelayAmount = 0;

    public EntityProcessor(string name="",string tag=""):base(name,tag)
    {

    }

    [NonSerialized]
    protected bool IsProcessing = false;
    
    public abstract Task BeginProcessAsync();
    
    public virtual void ConfigureDelay(int postDelayAmount)
    {          
        this.postDelayAmount = postDelayAmount;
    }

    public virtual void ResetDelay()
    {           
        this.postDelayAmount = 0;
    }

    protected virtual async Task AddDelay(int delayAmount)
    {
        if (delayAmount > 0)
        {
            logger.Debug($"Wait for {delayAmount / 1000.0} seconds");
            await Task.Delay(delayAmount);
        }
    }

    public void ResetComponents()
    {
        logger.Debug("Reset initiated for processor :  {this}", this);
        this.ResetHierarchy();
        logger.Debug("Reset completed for processor : {this}", this);

    }

    public override Entity AddComponent(IComponent component)
    {
        if (component is Entity)
        {
            base.AddComponent(component);
            return this;
        }

        throw new ArgumentException($"Only Entities can be added to a Entity Processor");
    }
      
    protected virtual async Task<bool> ProcessEntity(Entity targetEntity)
    {
        var entitiesBeingProcessed = new Stack<Entity>();
        IComponent actorBeingProcessed = null;
        try
        {
            await targetEntity.BeforeProcessAsync();
            foreach (IComponent component in targetEntity.GetNextComponentToProcess())
            {
                if (component.IsEnabled)
                {
                    logger.Debug("Component : [{$Id},{$Name}] will be processed next.", component.Id, component.Name);

                    switch (component)
                    {                           
                        case ActorComponent actor:
                            try
                            {
                                actorBeingProcessed = actor;                                   
                                actor.IsExecuting = true;
                                await actor.ActAsync();
                                await AddDelay(this.postDelayAmount);
                                await actor.OnCompletionAsync();
                            }
                            catch (Exception ex)
                            {
                                if (!actor.ContinueOnError)
                                {
                                    throw;
                                }
                                else
                                {
                                    actor.IsFaulted = true;
                                    logger.Warning(ex, ex.Message);
                                }
                            }
                            actor.IsExecuting = false;
                            break;

                        case IEntityProcessor processor:
                            processor.ConfigureDelay(this.postDelayAmount); 
                            await processor.BeginProcessAsync();
                            processor.ResetDelay();
                            break;

                        case Entity entity:
                            //Entity -> GetNextComponentToProcess yields child entity two times . Before processing its children and after it's children are processed
                            if (entitiesBeingProcessed.Count() > 0 && entitiesBeingProcessed.Peek().Equals(entity))
                            {
                                var processedEntity = entitiesBeingProcessed.Pop();
                                await processedEntity.OnCompletionAsync();
                            }
                            else
                            {
                                entitiesBeingProcessed.Push(entity);
                                await entity.BeforeProcessAsync();                                   
                            }
                            break;
                    }
                }
            }
            await targetEntity.OnCompletionAsync();

            logger.Debug("All components have been processed for Entity : {Id},{Name}", targetEntity.Id, targetEntity.Name);

        }
        catch (Exception ex)
        {
            logger.Error(ex.Message, ex);
            if (actorBeingProcessed is ActorComponent)
            {
                ActorComponent currentActor = actorBeingProcessed as ActorComponent;
                currentActor.IsExecuting = false;
                currentActor.IsFaulted = true;
            }

            while (entitiesBeingProcessed.Count() > 0)
            {
                try
                {
                    var entity = entitiesBeingProcessed.Pop();
                    await entity.OnFaultAsync(actorBeingProcessed);
                }
                catch (Exception faultHandlingExcpetion)
                {
                    logger.Error(faultHandlingExcpetion.Message, faultHandlingExcpetion);
                }
            }

            await targetEntity.OnFaultAsync(actorBeingProcessed);
            throw;
        }

        return true;
    }
}
