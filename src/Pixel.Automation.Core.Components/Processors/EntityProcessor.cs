﻿using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Components.Processors
{
    [DataContract]
    [Serializable]
    public abstract class EntityProcessor : Entity, IEntityProcessor
    {
        private readonly ILogger logger = Log.ForContext<EntityProcessor>();

        [DataMember]
        [Display(Name = "Processing Delay", GroupName = "Configuration", Order = 10)]
        [Description("Delay between execution of two actors")]
        public Argument ProcessingDelay { get; set; } = new InArgument<double>() { DefaultValue = 500, CanChangeType = false };

        public EntityProcessor(string name="",string tag=""):base(name,tag)
        {

        }

        [NonSerialized]
        protected bool IsProcessing = false;
        
        public abstract Task BeginProcess();
       
        public void ResetComponents()
        {
            logger.Information("Reset initiated for processor :  {this}", this);
            this.ResetHierarchy();
            logger.Information("Reset completed for processor : {this}", this);

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
                        logger.Information("Component : [{$Id},{$Name}] will be processed next.", component.Id, component.Name);


                        switch (component)
                        {
                            case ActorComponent actor:
                                try
                                {
                                    actorBeingProcessed = actor;                                   
                                    actor.IsExecuting = true;                                  
                                    actor.Act();                                   
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

                                //Thread.Sleep(TimeSpan.FromSeconds(this.ProcessingDelay));
                                actor.IsExecuting = false;
                                break;

                            case AsyncActorComponent actor:
                                try
                                {
                                    actorBeingProcessed = actor;                                 
                                    actor.IsExecuting = true;
                                    await actor.ActAsync();                                   
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

                                //Thread.Sleep(TimeSpan.FromSeconds(this.ProcessingDelay));
                                actor.IsExecuting = false;
                                break;

                            case IEntityProcessor processor:
                                await processor.BeginProcess();
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

                logger.Information("All components have been processed for Entity : {Id},{Name}", targetEntity.Id, targetEntity.Name);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
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
}
