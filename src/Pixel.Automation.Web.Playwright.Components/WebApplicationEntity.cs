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
    [Display(Name = "Context Options", GroupName = "Overrides", Order = 20, Description = "[Optional] Specify a custom browser new context options." +
      " Default configuration is used if not specified.")]
    public Argument ContextOptions { get; set; } = new InArgument<BrowserNewContextOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Optional argument which can be used to override the target url configured on application.
    /// </summary>
    [DataMember]
    [Display(Name = "Target Url", GroupName = "Overrides", Order = 30, Description = "[Optional] Override the Target Url option set on Application")]
    public Argument TargetUriOverride { get; set; } = new InArgument<string>() { CanChangeType = false, Mode = ArgumentMode.DataBound };


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

        Browsers preferredBrowser = await GetPreferredBrowser(webApplicationDetails);

        InstallBrowser(preferredBrowser);

        webApplicationDetails.Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browserLaunchOptions = await GetBrowserLaunchOptions();
        switch (preferredBrowser)
        {
            case Browsers.FireFox:
                webApplicationDetails.Browser = await webApplicationDetails.Playwright.Firefox.LaunchAsync(browserLaunchOptions);
                break;
            case Browsers.Chrome:
                webApplicationDetails.Browser = await webApplicationDetails.Playwright.Chromium.LaunchAsync(browserLaunchOptions);
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

        string goToUrl = webApplicationDetails.TargetUri.ToString();
        if (this.TargetUriOverride.IsConfigured())
        {
            goToUrl = await this.ArgumentProcessor.GetValueAsync<string>(this.TargetUriOverride);
            logger.Information($"TargetUri was over-ridden to {goToUrl} for application : {applicationDetails.ApplicationName}");
        }
        await webApplicationDetails.ActivePage.GotoAsync(goToUrl);

        logger.Information("{browserToLaunch} has been launched.", preferredBrowser);
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

    /// <summary>
    /// Get BrowserTypeLaunchOptions for browser
    /// </summary>        
    /// <returns></returns>
    async Task<BrowserTypeLaunchOptions> GetBrowserLaunchOptions()
    {
        if (this.LaunchOptions.IsConfigured())
        {
            var driverOptions =  await this.ArgumentProcessor.GetValueAsync<BrowserTypeLaunchOptions>(this.LaunchOptions);
            logger.Information($"Browser launch options was over-ridden for application : {applicationDetails.ApplicationName}");
            return driverOptions;
        }
        return new BrowserTypeLaunchOptions() { Headless = false };
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

    void InstallBrowser(Browsers preferredBrowser)
    {
        Program.Main(new[] { "install", "--help" });
        int exitCode = -1;
        switch (preferredBrowser)
        {
            case Browsers.FireFox:
                exitCode = Program.Main(new[] { "install", "firefox" });
                break;
            case Browsers.Chrome:
                exitCode = Program.Main(new[] { "install", "chromium" });
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
    }
}
