using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Processors
{
    [DataContract]
    [Serializable]
    //[ToolBoxItem("Async Processor", "Entity Processor", iconSource: null, description: "Process it's child entities asynchronously allowing parent processor to continue processing it's next components ", tags: new string[] { "Asynchronous processor" })]
    public class AsyncEntityProcessor : EntityProcessor
    {
        public AsyncEntityProcessor():base("Async Processor", "Processor")
        {

        }
        public override async Task BeginProcess()
        {          
            //TODO : Since Test Runner doesn't know when this will finish, Test Runner continues with next execution.
            //Also, We need to consider cancellation. Hiding this from toolbox until these are sorted.
            Task processorTask = new Task(async () =>
            {
                await ProcessEntity(this);

            });          
            processorTask.Start();
            await Task.CompletedTask;
        }

    }
}
