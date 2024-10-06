using Pixel.Windows.Automation;
using NUnit.Framework;

namespace UIAComWrapperTests;


/// <summary>
///This is a test class for CacheRequestTest and is intended
///to contain all CacheRequestTest Unit Tests
///</summary>
[TestFixture]
public class CacheRequestTest
{
    /// <summary>
    ///A test for TreeScope
    ///</summary>
    [Test]
    public void TreeScopeTest()
    {
        CacheRequest target = new CacheRequest();
        TreeScope expected = TreeScope.Subtree;
        TreeScope actual;
        target.TreeScope = expected;
        actual = target.TreeScope;
        Assert.That(expected, Is.EqualTo(actual));
    }

    /// <summary>
    ///A test for TreeFilter
    ///</summary>
    [Test]
    public void TreeFilterTest()
    {
        CacheRequest target = new CacheRequest();
        PropertyCondition expected = new PropertyCondition(AutomationElement.NameProperty, "foo");
        PropertyCondition actual;
        target.TreeFilter = expected;
        actual = (PropertyCondition)target.TreeFilter;
        Assert.That(actual.Flags, Is.EqualTo(expected.Flags));
        Assert.That(actual.Property, Is.EqualTo(expected.Property));
        Assert.That(actual.Value, Is.EqualTo(expected.Value));
    }

    /// <summary>
    ///A test for Current
    ///</summary>
    [Test]
    public void CurrentTest()
    {
        // We expect the Current one at this point to be the Default one
        CacheRequest actual;
        actual = CacheRequest.Current;
        Assert.That(actual is not null);
        Assert.That(actual.AutomationElementMode, Is.EqualTo(AutomationElementMode.Full));
        Assert.That(actual.TreeScope, Is.EqualTo(TreeScope.Element));
        Assert.That(actual.TreeFilter is not null);

        Assert.That(actual.TreeFilter is NotCondition);
        NotCondition notCond = (NotCondition)actual.TreeFilter;
        Assert.That(notCond.Condition is PropertyCondition);
        PropertyCondition propCond = (PropertyCondition)notCond.Condition;
        Assert.That(propCond.Property, Is.EqualTo(AutomationElement.IsControlElementProperty));
        Assert.That(propCond.Value, Is.EqualTo(false));
    }

    /// <summary>
    ///A test for AutomationElementMode
    ///</summary>
    [Test]
    public void AutomationElementModeTest()
    {
        CacheRequest target = new CacheRequest(); 
        target.AutomationElementMode = AutomationElementMode.Full;
        AutomationElementMode actual = target.AutomationElementMode;
        Assert.That(actual, Is.EqualTo(AutomationElementMode.Full));
    }

    /// <summary>
    ///A test for Push and Pop
    ///</summary>
    [Test]
    public void PushPopTest()
    {
        CacheRequest defaultCR = CacheRequest.Current;
        CacheRequest target = new CacheRequest();
        target.TreeScope = TreeScope.Children;
        target.Push();
        CacheRequest target2 = new CacheRequest();
        target2.TreeScope = TreeScope.Subtree;
        target2.Push();

        // Try to change target2 - this should fail
        try
        {
            target2.TreeScope = TreeScope.Descendants;

            Assert.Fail("exception expected");
        }
        catch (System.InvalidOperationException)
        {
        }

        target2.Pop();
        target.Pop();
        Assert.That(CacheRequest.Current, Is.EqualTo(defaultCR));
    }

    /// <summary>
    ///A test for Clone
    ///</summary>
    [Test]
    public void CloneTest()
    {
        CacheRequest target = new CacheRequest();
        target.TreeScope = TreeScope.Subtree;
        CacheRequest actual;
        actual = target.Clone();
        Assert.That(target.TreeScope, Is.EqualTo(actual.TreeScope));
    }

    /// <summary>
    ///A test for Add
    ///</summary>
    [Test]
    public void AddTest()
    {
        CacheRequest target = new CacheRequest(); 
        target.Add(AutomationElement.HelpTextProperty);
        target.Add(ExpandCollapsePatternIdentifiers.Pattern);
    }

    /// <summary>
    ///A test for Activate
    ///</summary>
    [Test]
    public void ActivateTest()
    {
        CacheRequest target = new CacheRequest();
        Assert.That(CacheRequest.Current, Is.Not.EqualTo(target));
        using (target.Activate())
        {
            Assert.That(CacheRequest.Current, Is.EqualTo(target));
            CacheRequest target2 = new CacheRequest();
            using (target2.Activate())
            {
                Assert.That(CacheRequest.Current, Is.Not.EqualTo(target));
            }
        }
    }
}
