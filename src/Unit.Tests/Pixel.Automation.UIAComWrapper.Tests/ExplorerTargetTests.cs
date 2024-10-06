using System;
using Pixel.Windows.Automation;
using NUnit.Framework;

namespace UIAComWrapperTests;

/// <summary>
/// Tests that use Explorer itself as a target
/// </summary>
[TestFixture]
public class ExplorerTargetTests
{
    private static ExplorerHost explorerHost;

    [OneTimeSetUp]
    public static void MyClassInitialize()
    {
        ExplorerTargetTests.explorerHost = new ExplorerHost();
    }

    [OneTimeTearDown]
    public static void MyClassCleanup()
    {
        ((IDisposable)ExplorerTargetTests.explorerHost).Dispose();
    }

    [SetUp]
    public void MyTestInitialize() 
    {
        ExplorerTargetTests.explorerHost.Element.SetFocus();
    }

    [Test]
    public void GridPatternTest()
    {
        AutomationElement itemsView = ExplorerTargetTests.explorerHost.Element.FindFirst(TreeScope.Subtree,
            new PropertyCondition(AutomationElement.ClassNameProperty, "UIItemsView"));
        Assert.That(itemsView is not null);

        // Try out the Grid Pattern
        GridPattern grid = (GridPattern)itemsView.GetCurrentPattern(GridPattern.Pattern);
        Assert.That(grid.Current.ColumnCount > 0);
        Assert.That(grid.Current.RowCount > 0);

        // GridItem
        AutomationElement gridItemElement = grid.GetItem(0, 0);
        Assert.That(gridItemElement is not null);
        GridItemPattern gridItem = (GridItemPattern)gridItemElement.GetCurrentPattern(GridItemPattern.Pattern);
        Assert.That(gridItem.Current.Row, Is.EqualTo(0));
        Assert.That(gridItem.Current.Column, Is.EqualTo(0));
        Assert.That(itemsView, Is.EqualTo(gridItem.Current.ContainingGrid));
    }

    public void GridPatternCachedTest()
    {
        CacheRequest req = new CacheRequest();
        req.Add(GridItemPattern.Pattern);
        req.Add(GridPattern.Pattern);
        req.Add(GridPattern.RowCountProperty);
        req.Add(GridPattern.ColumnCountProperty);
        req.Add(GridItemPattern.RowProperty);
        req.Add(GridItemPattern.ColumnProperty);
        req.Add(GridItemPattern.ContainingGridProperty);

        using (req.Activate())
        {
            AutomationElement itemsView = ExplorerTargetTests.explorerHost.Element.FindFirst(TreeScope.Subtree,
                new PropertyCondition(AutomationElement.ClassNameProperty, "UIItemsView"));
            Assert.That(itemsView is not null);

            // Try out the Grid Pattern
            GridPattern grid = (GridPattern)itemsView.GetCachedPattern(GridPattern.Pattern);
            Assert.That(grid.Cached.ColumnCount > 0);
            Assert.That(grid.Cached.RowCount > 0);

            // GridItem
            AutomationElement gridItemElement = grid.GetItem(0, 0);
            Assert.That(gridItemElement is not null);
            GridItemPattern gridItem = (GridItemPattern)gridItemElement.GetCachedPattern(GridItemPattern.Pattern);
            Assert.That(gridItem.Cached.Row, Is.EqualTo(0));
            Assert.That(gridItem.Cached.Column, Is.EqualTo(0));
            Assert.That(gridItem.Cached.ContainingGrid, Is.EqualTo(itemsView));
        }
    }

    [Test]
    public void MultipleViewPatternTest()
    {
        CacheRequest req = new CacheRequest();
        req.Add(MultipleViewPattern.Pattern);
        req.Add(MultipleViewPattern.CurrentViewProperty);
        req.Add(MultipleViewPattern.SupportedViewsProperty);

        using (req.Activate())
        {
            AutomationElement itemsView = ExplorerTargetTests.explorerHost.Element.FindFirst(TreeScope.Subtree,
                new PropertyCondition(AutomationElement.ClassNameProperty, "UIItemsView"));
            Assert.That(itemsView is not null);

            MultipleViewPattern multiView = (MultipleViewPattern)itemsView.GetCachedPattern(MultipleViewPattern.Pattern);
            int[] supportedViews = multiView.Cached.GetSupportedViews();
            Assert.That(supportedViews.Length > 0);
            bool inSupportedViews = false;
            foreach (int view in supportedViews)
            {
                if (view == multiView.Cached.CurrentView)
                {
                    inSupportedViews = true;
                    break;
                }
                string viewName = multiView.GetViewName(view);
                Assert.That(viewName.Length > 0);
            }
            Assert.That(inSupportedViews);
        }
    }

    [Test]
    public void MultipleViewPatternCachedTest()
    {
        AutomationElement itemsView = ExplorerTargetTests.explorerHost.Element.FindFirst(TreeScope.Subtree,
            new PropertyCondition(AutomationElement.ClassNameProperty, "UIItemsView"));
        Assert.That(itemsView is not null);

        MultipleViewPattern multiView = (MultipleViewPattern)itemsView.GetCurrentPattern(MultipleViewPattern.Pattern);
        int[] supportedViews = multiView.Current.GetSupportedViews();
        Assert.That(supportedViews.Length > 0);
        bool inSupportedViews = false;
        foreach (int view in supportedViews)
        {
            if (view == multiView.Current.CurrentView)
            {
                inSupportedViews = true;
                break;
            }
            string viewName = multiView.GetViewName(view);
            Assert.That(viewName.Length > 0);
        }
        Assert.That(inSupportedViews);
    }

    [Test]
    public void TablePatternTest()
    {
        AutomationElement itemsView = ExplorerTargetTests.explorerHost.Element.FindFirst(TreeScope.Subtree,
            new PropertyCondition(AutomationElement.ClassNameProperty, "UIItemsView"));
        Assert.That(itemsView is not null);

        // TablePattern test
        TablePattern table = (TablePattern)itemsView.GetCurrentPattern(TablePattern.Pattern);
        Assert.That(table.Current.ColumnCount > 0);
        Assert.That(table.Current.RowCount > 0);
        Assert.That(table.Current.GetRowHeaders().Length == 0);
        Assert.That(table.Current.GetColumnHeaders().Length > 0);

        AutomationElement tableItemElement = table.GetItem(0, 0);
        TableItemPattern tableItem = (TableItemPattern)tableItemElement.GetCurrentPattern(TableItemPattern.Pattern);
        Assert.That(tableItem.Current.Row, Is.EqualTo(0));
        Assert.That(tableItem.Current.Column, Is.EqualTo(0));
        Assert.That(tableItem.Current.ContainingGrid, Is.EqualTo(itemsView));
        Assert.That(tableItem.Current.GetColumnHeaderItems().Length == 1);
        Assert.That(tableItem.Current.GetRowHeaderItems().Length == 0);
    }

    [Test]
    public void TablePatternCachedTest()
    {
        CacheRequest req = new CacheRequest();
        req.Add(TablePattern.Pattern);
        req.Add(TableItemPattern.Pattern);
        req.Add(GridPattern.Pattern);
        req.Add(GridItemPattern.Pattern);
        req.Add(GridPattern.RowCountProperty);
        req.Add(GridPattern.ColumnCountProperty);
        req.Add(GridItemPattern.RowProperty);
        req.Add(GridItemPattern.ColumnProperty);
        req.Add(GridItemPattern.ContainingGridProperty);
        req.Add(TablePattern.RowHeadersProperty);
        req.Add(TablePattern.ColumnHeadersProperty);
        req.Add(TableItemPattern.RowHeaderItemsProperty);
        req.Add(TableItemPattern.ColumnHeaderItemsProperty);
        using (req.Activate())
        {
            AutomationElement itemsView = ExplorerTargetTests.explorerHost.Element.FindFirst(TreeScope.Subtree,
                new PropertyCondition(AutomationElement.ClassNameProperty, "UIItemsView"));
            Assert.That(itemsView is not null);

            // TablePattern test
            TablePattern table = (TablePattern)itemsView.GetCachedPattern(TablePattern.Pattern);
            Assert.That(table.Cached.ColumnCount > 0);
            Assert.That(table.Cached.RowCount > 0);
            Assert.That(table.Cached.GetRowHeaders().Length == 0);
            Assert.That(table.Cached.GetColumnHeaders().Length > 0);

            AutomationElement tableItemElement = table.GetItem(0, 0);
            TableItemPattern tableItem = (TableItemPattern)tableItemElement.GetCachedPattern(TableItemPattern.Pattern);
            Assert.That(tableItem.Cached.Row, Is.EqualTo(0));
            Assert.That(tableItem.Cached.Column, Is.EqualTo(0));
            Assert.That(itemsView, Is.EqualTo(tableItem.Cached.ContainingGrid));
            Assert.That(tableItem.Cached.GetColumnHeaderItems().Length == 1);
            Assert.That(tableItem.Cached.GetRowHeaderItems().Length == 0);
        }
    }

    [Test]
    public void TreeIterationTest()
    {
        TreeWalker walker = new TreeWalker(Automation.ControlViewCondition);
        AutomationElement startingElement = ExplorerTargetTests.explorerHost.Element;
        AutomationElement iter = startingElement;
        iter = walker.GetFirstChild(iter);
        iter = walker.GetNextSibling(iter);
        iter = walker.GetParent(iter);
        Assert.That(iter, Is.EqualTo(startingElement));
        iter = walker.GetLastChild(iter);
        iter = walker.GetPreviousSibling(iter);
        iter = walker.GetParent(iter);
        Assert.That(iter, Is.EqualTo(startingElement));
    }

    [Test]
    [Ignore("Test is not working on Windows 8 due to changes in Explorer")]
    public void VirtualizedPatternTest()
    {
        AutomationElement itemsView = ExplorerTargetTests.explorerHost.Element.FindFirst(TreeScope.Subtree,
            new PropertyCondition(AutomationElement.ClassNameProperty, "UIItemsView"));
        Assert.That(itemsView is not null);

        // Get the container
        Assert.That((bool)itemsView.GetCurrentPropertyValue(AutomationElement.IsItemContainerPatternAvailableProperty));
        ItemContainerPattern container = (ItemContainerPattern)itemsView.GetCurrentPattern(ItemContainerPattern.Pattern);
        
        // Look for something we know is there and is probably below the fold
        AutomationElement item1 = container.FindItemByProperty(null, AutomationElement.NameProperty, "winver");
        Assert.That(item1 is not null);

        // Let's get another one
        AutomationElement item2 = container.FindItemByProperty(item1, AutomationElement.NameProperty, "xcopy");
        Assert.That(item2 is not null);

        // Check the bounding rect -- should be empty
        System.Windows.Rect rect1 = item2.Current.BoundingRectangle;
        Assert.That(rect1.Width, Is.EqualTo(0));
        Assert.That(rect1.Height, Is.EqualTo(0));

        // Get the virtualized pattern
        Assert.That((bool)item2.GetCurrentPropertyValue(AutomationElement.IsVirtualizedItemPatternAvailableProperty));
        VirtualizedItemPattern virtItem2 = (VirtualizedItemPattern)item2.GetCurrentPattern(VirtualizedItemPattern.Pattern);
        Assert.That(item2 is not null);

        // Realize the item and give the window a moment to scroll
        virtItem2.Realize();
        System.Threading.Thread.Sleep(100 /* ms */);

        // Check the bounding rect now - should not be empty
        System.Windows.Rect rect2 = item2.Current.BoundingRectangle;
        Assert.That(rect2.Width, Is.Not.EqualTo(0));
        Assert.That(rect2.Height, Is.Not.EqualTo(0));


    }
}
