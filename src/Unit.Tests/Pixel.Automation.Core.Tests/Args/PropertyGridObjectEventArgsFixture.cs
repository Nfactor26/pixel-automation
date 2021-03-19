using NUnit.Framework;
using Pixel.Automation.Core.Args;

namespace Pixel.Automation.Core.Tests.Args
{
    class PropertyGridObjectEventArgsFixture
    {
        [Test]
        public void ValidateThaPropertyGridObjectEventArgsCanBeInitialized()
        {
            var propertyGridObjectEventArgs = new PropertyGridObjectEventArgs(new Entity());
            Assert.IsNotNull(propertyGridObjectEventArgs.ObjectToDisplay);
            Assert.IsFalse(propertyGridObjectEventArgs.IsReadOnly);
        }
    }
}
