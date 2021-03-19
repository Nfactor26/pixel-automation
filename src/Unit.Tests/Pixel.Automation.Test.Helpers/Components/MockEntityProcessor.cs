using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Helpers
{
    public class MockEntityProcessor : Entity, IEntityProcessor
    {

        public MockEntityProcessor() : base()
        {

        }

        public MockEntityProcessor(string name, string tag) : base(name, tag)
        {

        }

        public Task BeginProcess()
        {
            return Task.CompletedTask;
        }

        public void ResetComponents()
        {
            
        }
    }
}
