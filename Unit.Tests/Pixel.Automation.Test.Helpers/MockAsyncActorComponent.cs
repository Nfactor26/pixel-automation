using Pixel.Automation.Core;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Helpers
{
    public class MockAsyncActorComponent : AsyncActorComponent
    {

        public MockAsyncActorComponent() : base()
        {

        }

        public MockAsyncActorComponent(string name, string tag) : base(name, tag)
        {

        }

        public override Task ActAsync()
        {
            return Task.CompletedTask;
        }
    }
}
