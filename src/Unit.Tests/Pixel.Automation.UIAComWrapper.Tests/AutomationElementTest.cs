using NUnit.Framework;
using System;
using System.Windows;
using Pixel.Windows.Automation;
using Condition = Pixel.Windows.Automation.Condition;

namespace UIAComWrapperTests;


/// <summary>
///This is a test class for AutomationElementTest and is intended
///to contain all AutomationElementTest Unit Tests
///</summary>
[TestFixture]
public class AutomationElementTest
{
    public static AutomationElement GetStartButton()
    {
        AndCondition cond = new AndCondition(
            new PropertyCondition(AutomationElement.AccessKeyProperty, "Ctrl+Esc"),
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));
        return AutomationElement.RootElement.FindFirst(TreeScope.Subtree, cond);
    }

    public static AutomationElement GetTaskbar()
    {
        PropertyCondition cond = new PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd");
        return AutomationElement.RootElement.FindFirst(TreeScope.Subtree, cond);
    }

    public static AutomationElement GetClock()
    {
        return GetTaskbar().FindFirst(TreeScope.Subtree,
            new PropertyCondition(AutomationElement.ClassNameProperty, "TrayClockWClass"));
    }

    /// <summary>
    ///A test for RootElement
    ///</summary>
    [Test]
    public void RootElementTest()
    {
        AutomationElement actual;
        actual = AutomationElement.RootElement;
        Assert.That(actual is not null);
        Assert.That(actual.GetCurrentPropertyValue(AutomationElement.ClassNameProperty), Is.EqualTo("#32769"));
        Assert.That(actual.GetCurrentPropertyValue(AutomationElement.NativeWindowHandleProperty), Is.EqualTo(0x10010));
    }

    /// <summary>
    /// Test retrieval of the focused element
    /// </summary>
    [Test]
    public void FocusedElementTest()
    {
        AutomationElement actual = AutomationElement.FocusedElement;
        Assert.That(actual is not null);
        Assert.That(actual.Current.HasKeyboardFocus);
    }

    /// <summary>
    ///A test for FromPoint
    ///</summary>
    [Test]
    public void FromPointTest()
    {
        Point pt = new Point(); 
        AutomationElement actual;
        actual = AutomationElement.FromPoint(pt);
        Assert.That(actual is not null);
    }

    /// <summary>
    ///A test for FromHandle
    ///</summary>
    [Test]
    public void FromHandleTest()
    {
        int rootHwnd = (int)AutomationElement.RootElement.GetCurrentPropertyValue(
            AutomationElement.NativeWindowHandleProperty);
        AutomationElement actual = AutomationElement.FromHandle((IntPtr)rootHwnd);
        Assert.That(actual, Is.EqualTo(AutomationElement.RootElement));
    }

    /// <summary>
    ///A test for Equals
    ///</summary>
    [Test]
    public void EqualsTest()
    {
        Point pt = new Point();
        AutomationElement actual1 = AutomationElement.FromPoint(pt);
        Assert.That(actual1 is not null);
        AutomationElement actual2 = AutomationElement.FromPoint(pt);
        Assert.That(actual2 is not null );
        Assert.That(actual1, Is.EqualTo(actual2));
    }

    /// <summary>
    ///A test for FindFirst
    ///</summary>
    [Test]
    public void FindFirstTest()
    {
        // Find a child
        CacheRequest cacheReq = new CacheRequest();
        cacheReq.Add(AutomationElement.NativeWindowHandleProperty);
        using (cacheReq.Activate())
        {
            Assert.That(cacheReq, Is.SameAs(CacheRequest.Current));
            AutomationElement actualCached = AutomationElement.RootElement.FindFirst(
                TreeScope.Children,
                Condition.TrueCondition);
            Assert.That(actualCached is not null);
            int nativeHwnd = (int)actualCached.GetCachedPropertyValue(AutomationElement.NativeWindowHandleProperty);
            Assert.That(nativeHwnd != 0);
        }
    }

    /// <summary>
    ///A test for FindAll
    ///</summary>
    [Test]
    public void FindAllTest()
    {
        // Find all children
        CacheRequest cacheReq = new CacheRequest();
        cacheReq.Add(AutomationElement.NameProperty);
        cacheReq.Add(AutomationElement.NativeWindowHandleProperty);
        using (cacheReq.Activate())
        {
            AutomationElementCollection actual = AutomationElement.RootElement.FindAll(
                TreeScope.Children,
                Condition.TrueCondition);
            Assert.That(actual is not null);
            Assert.That(actual.Count > 0);

            foreach (AutomationElement elem in actual)
            {
                Assert.That(elem is not null);
                int nativeHwnd = (int)elem.GetCachedPropertyValue(AutomationElement.NativeWindowHandleProperty);
                Assert.That(nativeHwnd != 0);
            } 
        }
    }

    [Test]
    public void GetClickablePointTest()
    {
        AutomationElement clock = GetClock();
        Point point = clock.GetClickablePoint();
        Assert.That(point.X > 0);
        Assert.That(point.Y > 0);
    }

    [Test]
    public void GetRuntimeIdTest()
    {
        int[] runtimeId = AutomationElement.RootElement.GetRuntimeId();
        Assert.That(runtimeId is not null);
        Assert.That(runtimeId.Length > 0);
    }

    [Test]
    public void GetUpdatedCacheTest()
    {
        AutomationElement elem = AutomationElement.RootElement;
        try
        {
            string name = elem.Cached.Name;
            Assert.Fail("expected exception");
        }
        catch (ArgumentException)
        {
        }

        CacheRequest req = new CacheRequest();
        req.Add(AutomationElement.NameProperty);
        AutomationElement refreshed = elem.GetUpdatedCache(req);
        string name2 = refreshed.Cached.Name;
    }

    /// <summary>
    /// Simple test to invoke the start menu and test GetCurrentPattern
    /// </summary>
    [Test]
    public void GetCurrentPatternTest()
    {
        LegacyIAccessiblePattern pattern = (LegacyIAccessiblePattern)GetTaskbar().GetCurrentPattern(LegacyIAccessiblePattern.Pattern);
        Assert.That(pattern is not null);
    }

    /// <summary>
    /// Simple test to invoke the start menu and test GetCachedPattern
    /// </summary>
    [Test]
    public void GetCachedPatternTest()
    {
        CacheRequest req = new CacheRequest();
        req.Add(LegacyIAccessiblePattern.Pattern);
        using (req.Activate())
        {
            LegacyIAccessiblePattern pattern = (LegacyIAccessiblePattern)GetTaskbar().GetCachedPattern(LegacyIAccessiblePattern.Pattern);
            Assert.That(pattern is not null);
        }
    }

    [Test]
    public void GetSupportedTest()
    {
        AutomationProperty[] properties = GetTaskbar().GetSupportedProperties();
        Assert.That(properties is not null);
        Assert.That(properties.Length > 0);
        foreach (AutomationProperty property in properties)
        {
            Assert.That(property is not null);
            Assert.That(property.ProgrammaticName is not null);
            string programmaticName = Automation.PropertyName(property);
            Assert.That(programmaticName is not null);
        }

        AutomationPattern[] patterns = GetTaskbar().GetSupportedPatterns();
        Assert.That(patterns is not null);
        Assert.That(patterns.Length > 0);
        foreach (AutomationPattern pattern in patterns)
        {
            Assert.That(pattern is not null);
            Assert.That(pattern.ProgrammaticName is not null);
            string programmaticName = Automation.PatternName(pattern);
            Assert.That(programmaticName is not null);
        }
    }

    [Test]
    public void CachedRelationshipTest()
    {
        CacheRequest req = new CacheRequest();
        req.TreeScope = TreeScope.Element | TreeScope.Children;
        using (req.Activate())
        {
            AutomationElement rootElement = AutomationElement.RootElement;
            AutomationElementCollection rootChildren = rootElement.CachedChildren;
            Assert.That(rootChildren is not null);
            Assert.That(rootChildren.Count > 0);

            AutomationElement firstChild = rootChildren[0];
            AutomationElement cachedParent = firstChild.CachedParent;
            Assert.That(cachedParent is not null);
            Assert.That(cachedParent, Is.EqualTo(rootElement));
        }
    }

    [Test]
    public void NotSupportedValueTest()
    {
        AutomationElement taskbar = GetTaskbar();
        // Pretty sure the start button doesn't support this
        object value = taskbar.GetCurrentPropertyValue(AutomationElement.ItemStatusProperty, true);
        Assert.That(value, Is.EqualTo(AutomationElement.NotSupported));
    }

    [Test]
    public void BoundaryRectTest()
    {
        System.Windows.Rect boundingRect = GetTaskbar().Current.BoundingRectangle;
        Assert.That(boundingRect.Width > 0);
        Assert.That(boundingRect.Height > 0);
    }

    [Test]
    public void CompareTest()
    {
        AutomationElement el1 = GetTaskbar();
        AutomationElement el2 = GetTaskbar();
        Assert.That(Automation.Compare((AutomationElement)null, (AutomationElement)null));
        Assert.That(Automation.Compare(null, el1) == false);
        Assert.That(Automation.Compare(el1, null) == false);
        Assert.That(Automation.Compare(el1, el2));
        Assert.That(Automation.Compare(el1.GetRuntimeId(), el2.GetRuntimeId()));
    }

    [Test]
    public void ElementNotAvailableTest()
    {
        AutomationElement element;
        try
        {
            element = AutomationElement.FromHandle((IntPtr)0xF00D);
            Assert.Fail("expected exception");
        }
        catch (Exception e)
        {
            Assert.That(e is ElementNotAvailableException);
        }
    }

    [Test]
    public void ProviderDescriptionTest()
    {
        string description = (string)AutomationElement.RootElement.GetCurrentPropertyValue(AutomationElement.ProviderDescriptionProperty);
        Assert.That(description is not null);
        Assert.That(description.Length > 0);
    }
}
