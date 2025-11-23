using Caliburn.Micro;
using Pixel.Automation.Core.Interfaces;
using Pixel.Windows.Automation;
using System.Windows.Forms;

namespace Pixel.Automation.UIA.Scrapper;

public class ControlScrapperHoverMode : UIAControlScrapper
{
    public override string DisplayName => "UIA Scrapper : (Track Hover)";

    public ControlScrapperHoverMode(IEventAggregator eventAggregator, IHighlightRectangleFactory highlightRectangleFactory, IScreenCapture screenCapture)
        : base(eventAggregator, highlightRectangleFactory, screenCapture)
    {
    }

    protected override AutomationElement GetTrackedElement()
    {
        System.Windows.Point point = new System.Windows.Point(Cursor.Position.X, Cursor.Position.Y);
        return AutomationElement.FromPoint(point);
    }
}
