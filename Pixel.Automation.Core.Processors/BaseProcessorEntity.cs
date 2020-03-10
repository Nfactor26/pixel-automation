using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Processors
{
    [DataContract]
    [Serializable]
    public abstract class BaseProcessorEntity : Entity, IEntityProcessor
    {
        [DataMember]
        [Display(Name = "Delay", GroupName = "Processor Configuration", Order = 10)]
        [Description("Delay in Seconds introduced between processing of two components by the processor e.g. 0.5 , 1.0 ,etc. This can be used to control" +
            "the speed of processing")]
        public double ProcessingDelay { get; set; } = 0.5D;

        protected Stack<Entity> entitiesBeingProcessed = new Stack<Entity>();

        public BaseProcessorEntity(string name="",string tag=""):base(name,tag)
        {

        }

        [NonSerialized]
        protected bool IsProcessing = false;
        
        public abstract Task BeginProcess();
       
        public void ResetChildComponents()
        {
            Log.Information("Reset has been initiated on processor :  {this}",this);           
            foreach(Entity entity in this.Entities)
            {
                if(entity.IsEnabled)
                {
                    entity.ResetHierarchy();
                }
            }
            Log.Information("Reset has been completed for processor : {this}", this);

        }

        public override Entity AddComponent(IComponent component)
        {
            if (component as Entity == null)
                throw new ConfigurationException("Only Entities can be added as direct child of Processor Entity");
            return base.AddComponent(component);
        }

        //TODO : If there is an excpetion while running actor and we throw ex , it is never catched by calling processors. Check this.
        //However, if we inline this method, it works properly
        protected async Task<bool> ProcessComponent(IComponent component)
        {
            if (component.IsEnabled)
            {
                Log.Debug("Component : [{$Id},{$Name}] will be processed next.", component.Id, component.Name);
              

                switch(component)
                {
                    case ActorComponent actor:
                        try
                        {
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
                        break;

                    case IEntityProcessor processor:                      
                        await processor.BeginProcess();                   
                        break;

                    case Entity entity:
                        if (this.entitiesBeingProcessed.Count() > 0 && this.entitiesBeingProcessed.Peek().Equals(component as Entity))
                        {
                            var processedEntity = this.entitiesBeingProcessed.Pop();
                            processedEntity.OnCompletion();
                        }
                        else
                        {
                            component.BeforeProcess();
                            this.entitiesBeingProcessed.Push(component as Entity);
                        }
                        break;
                }              
            }
            return await Task.FromResult(true);
        }

    }
}
