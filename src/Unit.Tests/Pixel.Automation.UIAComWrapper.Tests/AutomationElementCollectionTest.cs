using System.Collections;
using NUnit.Framework;
using Pixel.Windows.Automation;

namespace UIAComWrapperTests;

/// <summary>
///This is a test class for AutomationElementCollectionTest and is intended
///to contain all AutomationElementCollectionTest Unit Tests
///</summary>
[TestFixture]
public class AutomationElementCollectionTest
{
    private AutomationElementCollection testColl;

    [SetUp]
    public void MyTestInitialize()
    {
        // Get all children of the desktop for our target collection
        CacheRequest cacheReq = new CacheRequest();
        cacheReq.Add(AutomationElement.NameProperty);
        cacheReq.Add(AutomationElement.NativeWindowHandleProperty);
        using (cacheReq.Activate())
        {
            this.testColl = AutomationElement.RootElement.FindAll(
                TreeScope.Children,
                Condition.TrueCondition);
            Assert.That(this.testColl is not null);
            Assert.That(this.testColl.Count > 0);
        }
    }

    /// <summary>
    ///A test for Item
    ///</summary>
    [Test]
    public void ItemTest()
    {
        AutomationElement actual = this.testColl[0];
        Assert.That(actual is not null);
    }

    /// <summary>
    ///A test for Count
    ///</summary>
    [Test]
    public void CountTest()
    {
        Assert.That(this.testColl.Count > 0);
    }

    /// <summary>
    ///A test for GetEnumerator
    ///</summary>
    [Test]
    public void GetEnumeratorTest()
    {
        IEnumerator actual = this.testColl.GetEnumerator();
        int count = 0;
        while (actual.MoveNext())
        {
            AutomationElement elem = (AutomationElement)actual.Current;
            Assert.That(elem is not null);
            ++count;
        } 

        actual.Reset();
        actual.MoveNext();
        Assert.That(actual.Current, Is.EqualTo(this.testColl[0]));
    }

    /// <summary>
    ///A test for CopyTo
    ///</summary>
    [Test]
    public void CopyToTest()
    {
        AutomationElement[] array = new AutomationElement[this.testColl.Count+1];
        this.testColl.CopyTo(array, 1);
        for (int i = 0; i < this.testColl.Count; ++i)
        {
            Assert.That(this.testColl[i], Is.EqualTo(array[i + 1]));
        }
    }
}
