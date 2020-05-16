using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Test.Helpers
{
    [Builder(typeof(MockBuilder))]
    public class MockWhileLoopEntity : Entity, ILoop
    {
        public MockWhileLoopEntity() : base()
        {

        }

        public MockWhileLoopEntity(string name, string tag) : base(name, tag)
        {

        }

        public bool ExitCriteriaSatisfied { get; set; }
    }

    public class MockBuilder
    {

    }
}
