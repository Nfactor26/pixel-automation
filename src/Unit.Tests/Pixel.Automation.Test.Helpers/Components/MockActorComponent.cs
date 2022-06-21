using Pixel.Automation.Core;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Helpers
{
    public class MockActorComponent : ActorComponent
    {
        public MockActorComponent() : base()
        {

        }

        public MockActorComponent(string name, string tag) : base(name, tag)
        {

        }

        public override async Task ActAsync()
        {
            await Task.CompletedTask;
        }
    }
}
