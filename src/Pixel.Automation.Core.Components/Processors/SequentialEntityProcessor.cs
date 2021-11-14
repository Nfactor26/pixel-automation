using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Processors
{
    /// <summary>
    /// Sequential Entity Processor are used at design time for Prefab editors allowing execution of the Prefab.    
    /// </summary>
    [DataContract]
    [Serializable]    
    public class SequentialEntityProcessor : EntityProcessor
    {
        /// <summary>
        /// constructor
        /// </summary>
        public SequentialEntityProcessor() : base("Sequential Processor", "Processor")
        {

        }

        /// <inheritdoc/>
        public override async Task BeginProcess()
        {
            await ProcessEntity(this);
        }     
      
    }
}
