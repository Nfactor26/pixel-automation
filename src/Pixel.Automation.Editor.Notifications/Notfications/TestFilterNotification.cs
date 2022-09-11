namespace Pixel.Automation.Editor.Notifications;

/// <summary>
/// Test explorer can receive a Test filter notification message from other modules to a apply a filter on the tests.
/// Only tests matching filter criteria should be visible.
/// </summary>
public class TestFilterNotification
{
    public string FilterText { get; private set; }

    public TestFilterNotification(string filter)
    {
        this.FilterText = filter;
    }

    public TestFilterNotification(string key, string value)
    {
        this.FilterText = $"{key}:{value}";
    }
}
