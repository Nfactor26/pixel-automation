using Caliburn.Micro;
using Pixel.Automation.Core.Interfaces;
using Pixel.Windows.Automation;

namespace Pixel.Automation.UIA.Scrapper;

public class ControlScrapperFocusMode : UIAControlScrapper
{
    public override string DisplayName => "UIA Scrapper : (Track Focus)";

    public ControlScrapperFocusMode(IEventAggregator eventAggregator, IHighlightRectangleFactory highlightRectangleFactory, IScreenCapture screenCapture)
        : base(eventAggregator, highlightRectangleFactory, screenCapture)
    {
    }

    protected override AutomationElement GetTrackedElement()
    { 
        return AutomationElement.FocusedElement;
    }
}
