using Caliburn.Micro;
using Pixel.Automation.Appium.Components.Android;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Notifications;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Appium.Scrapper;

public class AppiumControlScrapper : PropertyChangedBase, IControlScrapper, IHandle<RepositoryApplicationOpenedEventArgs>
{
    private readonly ILogger logger = Log.ForContext<AppiumControlScrapper>();
    private readonly IEventAggregator eventAggregator;
  
    public string DisplayName { get; } = "Appium Native Scrapper";

    string targetApplicationId = string.Empty;

    bool isCapturing;
    public bool IsCapturing
    {
        get => isCapturing;
        set
        {
            isCapturing = value;
            NotifyOfPropertyChange(() => IsCapturing);
        }
    }

    public AppiumControlScrapper(IEventAggregator eventAggregator, IHighlightRectangleFactory highlightRectangleFactory, IScreenCapture screenCapture)
    {
        this.eventAggregator = eventAggregator;
        this.eventAggregator.SubscribeOnUIThread(this);
        this.highlightRectangleFactory = highlightRectangleFactory;    
    }

    public bool CanToggleScrapper
    {
        get
        {
            return !(string.IsNullOrEmpty(this.targetApplicationId));
        }

    }

    private readonly IHighlightRectangleFactory highlightRectangleFactory;

    ConcurrentQueue<ScrapedControl> capturedControls;

    ///</inheritdoc>
    public async Task ToggleCapture()
    {
        if (IsCapturing)
        {
            await StartCapture();
        }
        else
        {
            await StopCapture();
        }
    }


    ///</inheritdoc>
    public async Task StartCapture()
    { 
        capturedControls = new ();
        capturedControls.Clear();
        ScrapedControl scrapedControl = new ScrapedControl() {  ControlData = new AndroidNativeControlIdentity() };
        capturedControls.Enqueue(scrapedControl);
        await Task.CompletedTask;
        logger.Information("Scraping is started now.");
    }

    public async Task StopCapture()
    {      
        await eventAggregator.PublishOnUIThreadAsync(capturedControls.ToList<ScrapedControl>());    
        capturedControls = null;
        logger.Information("Win Scrapper has been stopped.");
    }

    public IEnumerable<Object> GetCapturedControls()
    {
        return capturedControls.ToArray();
    }

    public async Task HandleAsync(RepositoryApplicationOpenedEventArgs message, CancellationToken cancellationToken)
    {
        targetApplicationId = message.ApplicationId;
        NotifyOfPropertyChange(() => CanToggleScrapper);
        await Task.CompletedTask;
    }
}