using NUnit.Framework;
using Pixel.Automation.Core;

namespace Pixel.Automation.Editor.Notifications.Tests;

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
