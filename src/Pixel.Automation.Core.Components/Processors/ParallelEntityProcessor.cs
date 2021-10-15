using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components.Sequences;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Processors
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Parallel Processor", "Entity Processor", iconSource: null, description: "Process it's child entities parallely", tags: new string[] { "Parallel Processor" })]
    [NoDropTarget]
    public class ParallelEntityProcessor : EntityProcessor
    {
        private readonly ILogger logger = Log.ForContext<ParallelEntityProcessor>();

        public ParallelEntityProcessor() : base("Parallel Processor", "Processor")
        {

        }


        public override async Task BeginProcess()
        {
            var exceptions = new ConcurrentQueue<Exception>();

            var result = Parallel.ForEach(this.Entities, async entity =>
            {
                try
                {
                    await ProcessEntity(entity);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, ex.Message);
                    exceptions.Enqueue(ex);
                }
            });

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            await Task.CompletedTask;
        }
      
        public override void ResolveDependencies()
        {
            if (this.Components.Count() > 0)
            {
                return;
            }

            base.AddComponent(new SequenceEntity() { Name = $"Parallel Block #{this.Entities.Count() + 1}" });
            base.AddComponent(new SequenceEntity() { Name = $"Parallel Block #{this.Entities.Count() + 1}" });
        }

        public override Entity AddComponent(IComponent component)
        {
            if(component is SequenceEntity sequenceEntity)
            {
                base.AddComponent(sequenceEntity);
            }
            return this;
        }

    }
}
