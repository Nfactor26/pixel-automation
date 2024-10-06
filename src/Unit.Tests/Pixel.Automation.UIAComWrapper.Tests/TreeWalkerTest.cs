using Pixel.Windows.Automation;
using NUnit.Framework;

namespace UIAComWrapperTests;

/// <summary>
/// Summary description for TreeWalker
/// </summary>
[TestFixture]
public class TreeWalkerTest
{
    [Test]
    public void PreDefinedConditionsTest()
    {
        Condition rawView = Automation.RawViewCondition;
        Assert.That(rawView is not null);

        Condition controlView = Automation.ControlViewCondition;
        Assert.That(controlView is NotCondition);
        NotCondition notCond = (NotCondition)controlView;
        Assert.That(notCond.Condition is PropertyCondition);

        Condition contentView = Automation.ContentViewCondition;
        Assert.That(contentView is NotCondition);
        NotCondition notCond2 = (NotCondition)contentView;
        Assert.That(notCond2.Condition is OrCondition);
    }

    //
    // TreeIterationTest moved to ExplorerTargetTests.
    //
}
