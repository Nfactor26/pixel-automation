using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Sequences;
using System.Linq;

namespace Pixel.Automation.Editor.Core.ViewModels
{
    public class EntityProcessorViewModel : EntityComponentViewModel
    {
        public EntityProcessorViewModel(Entity model) : base(model)
        {
        }

        public EntityProcessorViewModel(Entity model, EntityComponentViewModel parent) : base(model, parent)
        {
           
        }

        public void AddParallelBlock()
        {
            base.AddComponent(new SequenceEntity() { Name = $"Parallel Block #{(this.Model as Entity).Components.Count() + 1}" });
        }
    }
}
