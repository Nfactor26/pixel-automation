using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Processors
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Sequential Processor", "Entity Processor", iconSource: null, description: "Process it's child entities sequentially ", tags: new string[] { "Sequential Processor" })]
    public class SequentialEntityProcessor : EntityProcessor
    {
       
        public SequentialEntityProcessor():base("Sequential Processor", "Processor")
        {

        }

        public override async Task BeginProcess()
        {
            await ProcessEntity(this);
        }     
      
    }
}
