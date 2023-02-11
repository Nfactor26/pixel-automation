using Caliburn.Micro;
using Notifications.Wpf.Core;
using Pixel.Automation.Appium.Components.Android;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Appium.Scrapper;

/// <summary>
/// Control scrapper for android native controls
/// </summary>
public class AndroidNativeControlScrapper : AppiumNativeControlScrapper
{
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="windowManager"></param>
    /// <param name="highlightRectangleFactory"></param>
    /// <param name="screenCapture"></param>
    /// <param name="notificationManager"></param>
    public AndroidNativeControlScrapper(IEventAggregator eventAggregator, IWindowManager windowManager, 
        IHighlightRectangleFactory highlightRectangleFactory, IScreenCapture screenCapture, INotificationManager notificationManager) 
        : base(eventAggregator, windowManager, highlightRectangleFactory, screenCapture, notificationManager)
    {
        this.DisplayName = "Android (Native)";
    }

    /// </inheritdoc>
    protected override InspectorViewModel CreateInspectorViewModel()
    {
        return new AndroidInspectorViewModel(this.highlightRectangle, this.notificationManager);
    }

    /// </inheritdoc>
    protected override ScrapedControl CaptureControlOnClick()
    {
        var focusedRect = this.inspector.SelectedControl.ScaledBoundingBox;        
        var selectedControl = this.inspector.SelectedControl.Control as AndroidMobileControl;
        var controlScreenShot = screenCapture.CaptureArea(new BoundingBox((int)focusedRect.X, (int)focusedRect.Y, (int)focusedRect.Width, (int)focusedRect.Height));
        ScrapedControl scrapedControl = new ScrapedControl()
        {
            ControlData = new AndroidNativeControlIdentity()
            {
                FindByStrategy = "Accessibility Id",
                Identifier = selectedControl.AccessibilityId
            },
            ControlImage = controlScreenShot
        };
        return scrapedControl;
    }
}