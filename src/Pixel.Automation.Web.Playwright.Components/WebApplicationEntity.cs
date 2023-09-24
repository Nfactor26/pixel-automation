using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

public class WebApplicationEntity : ApplicationEntity
{
    [Browsable(false)]
    public string Platform { get; set; } = "FireFox";

    /// <summary>
    /// Optional argument which can be used to override the preferred browser configured on application.
    /// </summary>
    [DataMember]
    [Display(Name = "Browser", GroupName = "Overrides", Order = 10, Description = "[Optional] Override the preferred browser option set on Application")]
    public Argument BrowserOverride { get; set; } = new InArgument<Browsers>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Optionally specify custom <see cref="BrowserTypeLaunchOptions"/> with which playwright browser should be launched.
    /// If LaunchOptions is not configured, default configuration is created and used.
    /// </summary>
    [DataMember]
    [Display(Name = "Launch Options", GroupName = "Overrides", Order = 20, Description = "[Optional] Specify a custom browser launch options." +
        " Default configuration is used if not specified.")]
    public Argument LaunchOptions { get; set; } = new InArgument<BrowserTypeLaunchOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound  };

    /// <summary>
    /// Optionally specify custom <see cref="BrowserNewContextOptions"/> with which a new browser context should be created.
    /// If ContextOptions is not configured, default configuration is created and used.
    /// </summary>
    [DataMember]
    [Display(Name = "Context Options", GroupName = "Overrides", Order = 30, Description = "[Optional] Specify a custom browser new context options." +
      " Default configuration is used if not specified.")]
    public Argument ContextOptions { get; set; } = new InArgument<BrowserNewContextOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Optional argument which can be used to override the target url configured on application.
    /// </summary>
    [DataMember]
    [Display(Name = "Target Url", GroupName = "Overrides", Order = 40, Description = "[Optional] Override the Target Url option set on Application")]
    public Argument TargetUriOverride { get; set; } = new InArgument<string>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Indicates if configured browser should be automatically installed
    /// </summary>
    [DataMember]
    [Display(Name = "Install Browser", GroupName = "Browser Management", Order = 50, Description = "Indicates if configured browser should be automatically installed")]
    public Argument AutoInstallBrowser { get; set; } = new InArgument<bool> { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = true };

    /// <summary>
    /// Remote debugging url to use to connect over CDP. Use this for automation of a WebView2 application
    /// </summary>
    [DataMember]
    [Display(Name = "Remote Debugging Url", GroupName = "WebView2", Order = 20, Description = "Remote debugging url to use e.g. http://localhost:port")]
    public Argument RemoteDebuggingUrl { get; set; } = new InArgument<string>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

    /// <summary>
    /// Get the TargetApplicationDetails and apply any over-rides to it
    /// </summary>
    /// <returns></returns>
    public override IApplication GetTargetApplicationDetails()
    {
        base.GetTargetApplicationDetails();
        var applicationDetails = this.applicationDetails as WebApplication;
        this.Platform = applicationDetails.PreferredBrowser.ToString();
        OnPropertyChanged(nameof(this.Platform));
        return applicationDetails;
    }


    /// <summary>
    /// Launch the browser using Selenium WebDriver
    /// </summary>
    public override async Task LaunchAsync()
    {
        var webApplicationDetails = this.GetTargetApplicationDetails<WebApplication>();
        if (webApplicationDetails.Browser != null)
        {
            logger.Warning($"{webApplicationDetails.ApplicationName} is already launched");
            return;
        }

        //For WebView2 process           
        if(this.RemoteDebuggingUrl.IsConfigured())
        {       
            var remoteDebuggingUrl = await this.ArgumentProcessor.GetValueAsync<string>(this.RemoteDebuggingUrl);          
            logger.Information("Connecting over cdp to remote debugging url : {0}", remoteDebuggingUrl);
            webApplicationDetails.Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            webApplicationDetails.Browser = await webApplicationDetails.Playwright.Chromium.ConnectOverCDPAsync(remoteDebuggingUrl);
            webApplicationDetails.ActiveContext = webApplicationDetails.Browser.Contexts[0];
            webApplicationDetails.ActivePage = webApplicationDetails.ActiveContext.Pages[0];
        }
        else
        {
            Browsers preferredBrowser = await GetPreferredBrowser(webApplicationDetails);
            var browserLaunchOptions = await GetBrowserLaunchOptions(preferredBrowser);

            logger.Information("Browser : {0} will be launched", preferredBrowser.ToString());

            await InstallBrowser(preferredBrowser, browserLaunchOptions);

            webApplicationDetails.Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            switch (preferredBrowser)
            {
                case Browsers.Chrome:
                case Browsers.Edge:
                    webApplicationDetails.Browser = await webApplicationDetails.Playwright.Chromium.LaunchAsync(browserLaunchOptions);
                    break;
                case Browsers.FireFox:
                    webApplicationDetails.Browser = await webApplicationDetails.Playwright.Firefox.LaunchAsync(browserLaunchOptions);
                    break;

                case Browsers.WebKit:
                    webApplicationDetails.Browser = await webApplicationDetails.Playwright.Webkit.LaunchAsync(browserLaunchOptions);
                    break;
                default:
                    throw new ArgumentException("Requested web driver type is not supported");
            }

            var browserContextOptions = await GetBrowserNewContextOptions();
            webApplicationDetails.ActiveContext = await webApplicationDetails.Browser.NewContextAsync(browserContextOptions);
            webApplicationDetails.ActivePage = await webApplicationDetails.ActiveContext.NewPageAsync();
            logger.Information("{browserToLaunch} has been launched.", preferredBrowser);

            string goToUrl = webApplicationDetails.TargetUri.ToString();
            if (this.TargetUriOverride.IsConfigured())
            {
                goToUrl = await this.ArgumentProcessor.GetValueAsync<string>(this.TargetUriOverride);
                logger.Information($"TargetUri was over-ridden to {goToUrl} for application : {applicationDetails.ApplicationName}");
            }
            await webApplicationDetails.ActivePage.GotoAsync(goToUrl);
            logger.Information("Browser was navigted to {0}", goToUrl);
        }     
    }

    /// <summary>
    /// Close browser by disposing the WebDriver
    /// </summary>
    public override async Task CloseAsync()
    {
        var webApplicationDetails = this.GetTargetApplicationDetails<WebApplication>();
        await webApplicationDetails.Browser?.CloseAsync();      
        webApplicationDetails.Playwright?.Dispose();
        webApplicationDetails.ActivePage = null;
        webApplicationDetails.ActiveContext = null;
        webApplicationDetails.Browser = null;
        webApplicationDetails.Playwright = null;
    }

    public override async Task CaptureScreenShotAsync(string filePath)
    {
        var activePage = this.GetTargetApplicationDetails<WebApplication>().ActivePage;
        await activePage.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = filePath,
            Type = ScreenshotType.Jpeg,
            Quality = 50
        });
    }

    /// <summary>
    /// Get BrowserTypeLaunchOptions for browser
    /// </summary>        
    /// <returns></returns>
    async Task<BrowserTypeLaunchOptions> GetBrowserLaunchOptions(Browsers preferredBrowser)
    {
        BrowserTypeLaunchOptions launchOptions = new BrowserTypeLaunchOptions() { Headless = false };
        if (this.LaunchOptions.IsConfigured())
        {
            launchOptions =  await this.ArgumentProcessor.GetValueAsync<BrowserTypeLaunchOptions>(this.LaunchOptions);           
            logger.Information($"Browser launch options was over-ridden for application : {applicationDetails.ApplicationName}");            
        }
        switch (preferredBrowser)
        {
            case Browsers.Chrome:
                if (string.IsNullOrEmpty(launchOptions.Channel))
                {
                    launchOptions.Channel = "chrome";
                }
                break;
            case Browsers.Edge:
                if (string.IsNullOrEmpty(launchOptions.Channel))
                {
                    launchOptions.Channel = "msedge";
                }
                break;
        }
        return launchOptions;
    }

    /// <summary>
    /// Get BrowserNewContextOptions for browser
    /// </summary>        
    /// <returns></returns>
    async Task<BrowserNewContextOptions> GetBrowserNewContextOptions()
    {
        if (this.ContextOptions.IsConfigured())
        {
            var contextOptions = await this.ArgumentProcessor.GetValueAsync<BrowserNewContextOptions>(this.ContextOptions);
            logger.Information($"Browser new context options was over-ridden for application : {applicationDetails.ApplicationName}");
            return contextOptions;
        }
        return new BrowserNewContextOptions() { ViewportSize = ViewportSize.NoViewport };
    }

    async  Task<Browsers> GetPreferredBrowser(WebApplication webApplication)
    {
        Browsers preferredBrowser = webApplication.PreferredBrowser;
        if (this.BrowserOverride.IsConfigured())
        {
            preferredBrowser = await this.ArgumentProcessor.GetValueAsync<Browsers>(this.BrowserOverride);
            logger.Information($"Preferred browser was over-ridden to {preferredBrowser} for application : {webApplication.ApplicationName}");            
        }
        return preferredBrowser;
    }

    async Task InstallBrowser(Browsers preferredBrowser, BrowserTypeLaunchOptions launchOptions)
    {
        bool shouldInstallBrowser = await this.ArgumentProcessor.GetValueAsync<bool>(this.AutoInstallBrowser);
        if(!shouldInstallBrowser)
        {
            logger.Information($"Browser installation will be skipped. Install Browser is configured as false.");
            return;
        }
        
        Program.Main(new[] { "install", "--help" });      
        int exitCode = 0;
        string channel;
        switch (preferredBrowser)
        {
            case Browsers.FireFox:                
                exitCode = Program.Main(new[] { "install", "firefox" });
                break;
            case Browsers.Chrome:
                //Don't install the stock chrome. It is expected to be present on system
                channel = launchOptions.Channel ?? "chrome";
                if(!string.Equals(channel, "chrome"))
                {
                    exitCode = Program.Main(new[] { "install", channel });
                }
                else
                {
                    //for stock browser, exitCode is non-zero if browser is already installed so we don't check it.
                    Program.Main(new[] { "install", channel });
                }
                break;
            case Browsers.Edge:
                //Don't install the stock edge. It is expected to be present on system
                channel = launchOptions.Channel ?? "msedge";
                if (!string.Equals(channel, "msedge"))
                {
                    exitCode = Program.Main(new[] { "install", channel });
                }
                else
                {
                    //for stock browser, exitCode is non-zero if browser is already installed so we don't check it.
                    Program.Main(new[] { "install", channel });
                }
                break;
            case Browsers.WebKit:               
                exitCode = Program.Main(new[] { "install", "webkit" });
                break;
            default:
                throw new ArgumentException("Requested web driver type is not supported");
        }
        if (exitCode != 0)
        {
            throw new Exception($"Playwright exited with code {exitCode}");
        }
        logger.Information("Browser: {0} was installed successfully.", preferredBrowser);

    }
}
