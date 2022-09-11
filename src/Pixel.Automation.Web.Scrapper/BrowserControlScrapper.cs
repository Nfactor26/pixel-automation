using Caliburn.Micro;
using Dawn;
using Microsoft.Playwright;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Notifications;
using Pixel.Automation.Web.Common;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Playwright.Scrapper;

/// <summary>
/// Implementation of <see cref="IControlScrapper"/> for capturing web control details from browser.
/// </summary>
public class BrowserControlScrapper : PropertyChangedBase, IControlScrapper, IHandle<RepositoryApplicationOpenedEventArgs>
{
    private readonly ILogger logger = Log.ForContext<BrowserControlScrapper>();

    string targetApplicationId = string.Empty;

    private readonly IEventAggregator eventAggregator;
    private readonly IScreenCapture screenCapture;
    private readonly ConcurrentQueue<ScrapedControl> capturedControls = new ConcurrentQueue<ScrapedControl>();

    IPlaywright playWright;
    IBrowserContext browserContext;
  
    ///</inheritdoc>      
    public string DisplayName => "Playwright Scrapper";

    bool isCapturing;
   
    ///</inheritdoc>   
    public bool IsCapturing
    {
        get => isCapturing;
        set
        {
            isCapturing = value;
            NotifyOfPropertyChange(() => IsCapturing);
        }
    }

    ///</inheritdoc>
    public bool CanToggleScrapper
    {
        get
        {
            return !(string.IsNullOrEmpty(this.targetApplicationId));
        }
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="screenCapture"></param>
    public BrowserControlScrapper(IEventAggregator eventAggregator, IScreenCapture screenCapture)
    {
        this.screenCapture = Guard.Argument(screenCapture).NotNull().Value;
        this.eventAggregator = Guard.Argument(eventAggregator).NotNull().Value;
        this.eventAggregator.SubscribeOnUIThread(this);           
    }


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
        capturedControls.Clear();
        InstallBrowser();
        playWright = await Microsoft.Playwright.Playwright.CreateAsync();
        string pathToExtension = Path.Combine(Environment.CurrentDirectory, "Extensions", "pixel-browser-scrapper");
        string dataDirectory = Path.Combine(Environment.CurrentDirectory, "Extensions", "Context");
        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }
        browserContext = await playWright.Chromium.LaunchPersistentContextAsync(dataDirectory, new BrowserTypeLaunchPersistentContextOptions()
        {
            Headless = false,
            Args = new List<string>()
            {
                $"--disable-extensions-except={pathToExtension}",
                $"--load-extension={pathToExtension}"
            },
            ViewportSize = ViewportSize.NoViewport
        });
        var page = browserContext.Pages[0];
        await page.GotoAsync("https://www.bing.com");
        page.Console += HandleMessagesOnConsole;

        logger.Information("Scraping is started now.");
    }

    /// <summary>
    /// Browser plugin will process the control and log on console. We subcribe to Playwright Console event to get the messages logged on browser console
    /// and process the captured control details.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HandleMessagesOnConsole(object sender, IConsoleMessage e)
    {
        try
        {
            if (e.Type == "log")
            {
                if(e.Text.Contains("controlLocation"))
                {
                    ScrapedControlData capturedData = JsonSerializer.Deserialize<ScrapedControlData>(e.Text, new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    }) ?? throw new NullReferenceException("Control details could not be deserialized");

                    var controlScreenShot = screenCapture.CaptureArea(new BoundingBox(capturedData.Left, capturedData.Top, capturedData.Width, capturedData.Height));
                    ScrapedControl scrapedControl = new ScrapedControl() { ControlImage = controlScreenShot, ControlData = capturedData };
                    capturedControls.Enqueue(scrapedControl);

                    logger.Information("Recevied control with identifier : {identifier}", capturedData.Identifier);
                    return;
                }

                if(e.Text.Equals("startListening"))
                {

                    foreach (var page in browserContext.Pages)
                    {
                        page.Console -= HandleMessagesOnConsole;
                        page.Console += HandleMessagesOnConsole;

                    }
                }              
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message, ex);
        }
    }

    /// <summary>
    /// Install chrome browser for use with scraping
    /// </summary>
    /// <exception cref="Exception"></exception>
    void InstallBrowser()
    {
        Program.Main(new[] { "install", "--help" });
        int exitCode = Program.Main(new[] { "install", "chromium" });
        if (exitCode != 0)
        {
            throw new Exception($"Playwright exited with code {exitCode}");
        }
    }

    ///</inheritdoc>
    public async Task StopCapture()
    {
        try
        {
            foreach(var page in browserContext.Pages)
            {
                page.Console -= HandleMessagesOnConsole;
            }
            await browserContext.CloseAsync();
            await browserContext.DisposeAsync();
            playWright.Dispose();
            await eventAggregator.PublishOnUIThreadAsync(capturedControls.ToList<ScrapedControl>());
            browserContext = null;
            logger.Information("Scrapping is stopped now.");
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message, ex);
        }       
    }

    ///</inheritdoc>
    public IEnumerable<Object> GetCapturedControls()
    {
        return capturedControls.ToArray();
    }

    /// <summary>
    /// Notification handler for <see cref="RepositoryApplicationOpenedEventArgs"/> message.
    /// The message signals that the scraping can be started.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task HandleAsync(RepositoryApplicationOpenedEventArgs message, CancellationToken cancellationToken)
    {
        targetApplicationId = message.ApplicationId;
        NotifyOfPropertyChange(() => CanToggleScrapper);
        await Task.CompletedTask;
    }
}