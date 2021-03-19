using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Processors
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Sequential Processor", "Entity Processor", iconSource: null, description: "Process it's child entities sequentially ", tags: new string[] { "Sequential Processor" })]
    public class SequentialProcessorEntity : BaseProcessorEntity
    {
       
        public SequentialProcessorEntity():base("Sequential Processor", "Processor")
        {

        }

        public override Task BeginProcess()
        {
           
            Task processorTask = new Task(async () =>
            {
                IsProcessing = true;
                OnPropertyChanged("CanResetHierarchy");

                Log.Information("Execution of processor : {this} has started.", this);

                IComponent actorBeingProcessed = null;
                try
                {
                    foreach (var component in this.GetNextComponentToProcess())
                    {
                        if (component.IsEnabled)
                        {


                            switch (component)
                            {
                                case ActorComponent actor:
                                    try
                                    {
                                        Log.Debug("Component : [{$Id},{$Name}] will be processed next.", component.Id, component.Name);

                                        actorBeingProcessed = actor;
                                        actor.BeforeProcess();
                                        actor.IsExecuting = true;
                                        actor.Act();
                                        actor.OnCompletion();
                                    }
                                    catch (Exception ex)
                                    {
                                        if (!actor.ContinueOnError)
                                        {
                                            // if (!component.OnFault(actorBeingProcessed))
                                            throw ex;
                                        }
                                        else
                                        {
                                            Log.Warning(ex, ex.Message);
                                        }
                                    }

                                    Thread.Sleep(TimeSpan.FromSeconds(this.ProcessingDelay));
                                    actor.IsExecuting = false;
                                    Log.Debug("Component : [{$Id},{$Name}] has been processed.", component.Id, component.Name);

                                    break;

                                case IEntityProcessor processor:
                                    Log.Debug("Processor : [{$Id},{$Name}] will be started now.", component.Id, component.Name);
                                    await processor.BeginProcess();
                                    Log.Debug("Processor : [{$Id},{$Name}] has finished processing.", component.Id, component.Name);
                                    break;

                                case Entity entity:
                                    if (this.entitiesBeingProcessed.Count() > 0 && this.entitiesBeingProcessed.Peek().Equals(component as Entity))
                                    {
                                        var processedEntity = this.entitiesBeingProcessed.Pop();
                                        processedEntity.OnCompletion();
                                        Log.Debug("Entity : [{$Id},{$Name}] has been processed.", component.Id, component.Name);

                                    }
                                    else
                                    {
                                        Log.Debug("Entity : [{$Id},{$Name}] will be processed next.", component.Id, component.Name);
                                        component.BeforeProcess();
                                        this.entitiesBeingProcessed.Push(component as Entity);
                                    }
                                    break;
                            }
                        }
                    }
                    Log.Information("Execution of processor : {this} has completed.",this);
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

                    while (this.entitiesBeingProcessed.Count() > 0)
                    {
                        try
                        {
                            var entity = this.entitiesBeingProcessed.Pop();
                            entity.OnFault(actorBeingProcessed);
                        }
                        catch(Exception faultHandlingExcpetion)
                        {
                            Log.Error(faultHandlingExcpetion.Message, faultHandlingExcpetion);
                        }
                    }

                }
                finally
                {
                    IsProcessing = false;
                    OnPropertyChanged("CanResetHierarchy");
                }
            },TaskCreationOptions.AttachedToParent);
            processorTask.Start();                      
            return processorTask;     
        }
      
      
    }
}
