using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.Extensions;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

public class WebApplicationEntity : ApplicationEntity
{
    [Browsable(false)]
    public string Platform { get; set; } = "FireFox";

    /// <summary>
    /// Indicates if a compatible version of webdriver should be automatically downloaded
    /// </summary>
    [DataMember]
    [Display(Name = "Auto Download", GroupName = "WebDriver", Order = 10, Description = "Indicate if a compatible driver version should be automatically downloaded using selenium-manager")]
    public Argument AutoDownloadDriver { get; set; } = new InArgument<bool> { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = true };

    /// <summary>
    /// Directory location for web driver binaries when auto download is false. Default value is ".//WebDrivers/" folder relative to application.
    /// Users must provide the web driver binaries at this location.
    /// </summary>
    [DataMember]
    [Display(Name = "Driver Location", GroupName = "WebDriver", Order = 20, Description = "Directory location for webdriver when auto download is false")]
    public Argument WebDriverLocation { get; set; } = new InArgument<string> { Mode = ArgumentMode.Default, CanChangeType = false, DefaultValue = ".//WebDrivers//" };   
    
    /// <summary>
    /// Optional argument which can be used to override the preferred browser configured on application.
    /// </summary>
    [DataMember]
    [Display(Name = "Browser", GroupName = "Overrides", Order = 30, Description = "[Optional] Override the preferred browser option set on Application")]
    public Argument BrowserOverride { get; set; } = new InArgument<Browsers>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

    [DataMember]
    [Display(Name = "Driver Service", GroupName = "Overrides", Order = 40, Description = "[Optional] Specify a custom driver service")]
    public Argument DriverServiceOverride { get; set; } = new InArgument<DriverService> { Mode = ArgumentMode.DataBound, CanChangeType = false };

    /// <summary>
    /// Optionally specify custom <see cref="DriverOptionsOverride"/> with which WebDriver should be initialized.
    /// If DriverOptions is not configured, default configuration is created and used.
    /// </summary>
    [DataMember]
    [Display(Name = "Driver Options", GroupName = "Overrides", Order = 50, Description = "[Optional] Specify a custom configured driver options." +
        "Default configuration is used if not specified.")]
    public Argument DriverOptionsOverride { get; set; } = new InArgument<DriverOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Webdriver command timeout
    /// </summary>
    [DataMember]
    [Display(Name = "Command Timeout", GroupName = "Overrides", Order = 60, Description = "[Optional] Webdriver command timeout")]
    public Argument CommandTimout { get; set; } = new InArgument<TimeSpan>() { CanChangeType = false, DefaultValue = TimeSpan.FromSeconds(60) };

    /// <summary>
    /// Optional argument which can be used to override the target url configured on application.
    /// </summary>
    [DataMember]
    [Display(Name = "Target Url", GroupName = "Overrides", Order = 70, Description = "[Optional] Override the Target Url option set on Application")]
    public Argument TargetUriOverride { get; set; } = new InArgument<Uri>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Optional argument which indicates whether to use selenium grid
    /// </summary>
    [DataMember]
    [Display(Name = "Use Grid", GroupName = "Grid", Order = 80, Description = "Indicates whether to use selenium grid")]
    public Argument UseGridOverride { get; set; } = new InArgument<bool>() { CanChangeType = false, AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Optional argument for selenium grid server url
    /// </summary>
    [DataMember]
    [Display(Name = "Grid Url", GroupName = "Grid", Order = 90, Description = "Url of the selenium grid server")]
    public Argument GridUrlOverride { get; set; } = new InArgument<Uri>() { CanChangeType = false, AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

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
        if (webApplicationDetails.WebDriver != null)
        {
            logger.Warning($"{webApplicationDetails.ApplicationName} is already launched");
            await Task.CompletedTask;
            return;
        }

        string processIdentifier = Guid.NewGuid().ToString();
      
        Browsers preferredBrowser = webApplicationDetails.PreferredBrowser;
        if (this.BrowserOverride.IsConfigured())
        {
            preferredBrowser = await this.ArgumentProcessor.GetValueAsync<Browsers>(this.BrowserOverride);
            logger.Information($"Preferred browser was over-ridden to {preferredBrowser} for application : {webApplicationDetails.ApplicationName}");
        }
        var commandTimeout = await this.ArgumentProcessor.GetValueAsync<TimeSpan>(this.CommandTimout);
        bool useSelniumGrid = this.UseGridOverride.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<bool>(this.UseGridOverride) : webApplicationDetails.UseGrid;
        if(useSelniumGrid)
        {
            var remoteDriverUri = this.GridUrlOverride.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<Uri>(this.GridUrlOverride) : webApplicationDetails.GridUrl;
            if(string.IsNullOrEmpty(remoteDriverUri.AbsoluteUri))
            {
                throw new ArgumentNullException("Selenium grid url is not configured");
            }
            var driverOptions = await GetDriverOptions<DriverOptions>(preferredBrowser, processIdentifier);
            webApplicationDetails.WebDriver = new RemoteWebDriver(remoteDriverUri, driverOptions.ToCapabilities(), commandTimeout);
        }
        else
        {           
            switch (preferredBrowser)
            {
                case Browsers.FireFox:
                    var fireFoxDriverOptions = await GetDriverOptions<FirefoxOptions>(preferredBrowser, processIdentifier);
                    var fireFoxDriverService = await GetDriverService<FirefoxDriverService>(preferredBrowser, fireFoxDriverOptions);
                    webApplicationDetails.WebDriver = new FirefoxDriver(fireFoxDriverService, fireFoxDriverOptions, commandTimeout);
                    break;
                case Browsers.Chrome:
                    var chromeDriverOptions = await GetDriverOptions<ChromeOptions>(preferredBrowser, processIdentifier);
                    var chromeDriverService = await GetDriverService<ChromeDriverService>(preferredBrowser, chromeDriverOptions);
                    webApplicationDetails.WebDriver = new ChromeDriver(chromeDriverService, chromeDriverOptions, commandTimeout);
                    break;
                case Browsers.Edge:
                    var edgeDriverOptions = await GetDriverOptions<EdgeOptions>(preferredBrowser, processIdentifier);
                    var edgeDriverService = await GetDriverService<EdgeDriverService>(preferredBrowser, edgeDriverOptions);
                    webApplicationDetails.WebDriver = new EdgeDriver(edgeDriverService, edgeDriverOptions, commandTimeout);
                    break;
                default:
                    throw new ArgumentException($"Browser : '{preferredBrowser}' is not supported");
            }
            logger.Information("{browserToLaunch} has been launched.", preferredBrowser);
        }

        if (webApplicationDetails.MaximizeOnLaunch)
        {
            webApplicationDetails.WebDriver.Manage().Window.Maximize();
        }

        //TODO : System.Management complains of only supported on windows desktop application although we are running on window desktop applications
        //for firefox/chrome
        //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //{
        //    var launchedInstance = GetLaunchedBrowserProcess(preferredBrowser, processIdentifier);
        //    webApplicationDetails.TargetApplication = ApplicationProcess.Attach(launchedInstance);
        //    logger.Information($"Process Id of launched browser is : {webApplicationDetails.ProcessId}");
        //}

        Uri goToUrl = webApplicationDetails.TargetUri;
        if (this.TargetUriOverride.IsConfigured())
        {
            goToUrl = await this.ArgumentProcessor.GetValueAsync<Uri>(this.TargetUriOverride);
            logger.Information($"TargetUri was over-ridden to {goToUrl} for application : {applicationDetails.ApplicationName}");
        }
        if(!string.IsNullOrEmpty(goToUrl?.OriginalString))
        {
            webApplicationDetails.WebDriver.Navigate().GoToUrl(goToUrl);
        }
        logger.Information($"{preferredBrowser} has been navigated to {goToUrl}");
    }

    /// <summary>
    /// Close browser by disposing the WebDriver
    /// </summary>
    public override async Task CloseAsync()
    {
        var webApplicationDetails = this.GetTargetApplicationDetails<WebApplication>();
        if (webApplicationDetails.WebDriver != null)
        {
            webApplicationDetails.WebDriver.Quit();
            webApplicationDetails.WebDriver = null;
            await Task.CompletedTask;
        }
    }

    public override async Task CaptureScreenShotAsync(string filePath)
    {
        var webDriver = this.GetTargetApplicationDetails<WebApplication>().WebDriver;
        webDriver.TakeScreenshot().SaveAsFile(filePath, ScreenshotImageFormat.Png);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Get DriverOptions for WebDriver based on PreferredBrowser
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="browser"></param>
    /// <param name="processIdentifier"></param>
    /// <returns></returns>
    async Task<T> GetDriverOptions<T>(Browsers browser, string processIdentifier) where T : DriverOptions
    {
        DriverOptions driverOptions = default;
        if (this.DriverOptionsOverride.IsConfigured())
        {
            driverOptions = await this.ArgumentProcessor.GetValueAsync<DriverOptions>(this.DriverOptionsOverride);
            logger.Information($"DriverOptions was over-ridden for application : {applicationDetails.ApplicationName}");
            if (!(driverOptions is T))
            {
                throw new ArgumentException($"Incorrect driver option  over-ride : {driverOptions.GetType()} for browser : {browser}");
            }
        }

        //Create a default driver option based on browser type
        switch (browser)
        {
            case Browsers.FireFox:
                FirefoxOptions firefoxOptions = driverOptions as FirefoxOptions ?? new FirefoxOptions();
                firefoxOptions.AddArgument($"--{processIdentifier}");
                return firefoxOptions as T;
            case Browsers.Chrome:
                ChromeOptions chromeOptions = driverOptions as ChromeOptions ?? new ChromeOptions();
                //https://github.com/GoogleChrome/chrome-launcher/blob/main/docs/chrome-flags-for-tools.md
                chromeOptions.AddArgument("test-type");
                chromeOptions.AddArgument(processIdentifier);
                return chromeOptions as T;
            case Browsers.Edge:
                EdgeOptions edgeOptions = driverOptions as EdgeOptions ?? new EdgeOptions();
                //edgeOptions.AddArgument("test-type");
                edgeOptions.AddArgument($"--{processIdentifier}");
                return edgeOptions as T;
            default:
                throw new ArgumentException("Requested web driver type is not supported");
        }
    }

    async Task<T> GetDriverService<T>(Browsers browser , DriverOptions driverOptions) where T : DriverService
    {
        DriverService driverService;
        if (this.DriverServiceOverride.IsConfigured())
        {
            driverService = await this.ArgumentProcessor.GetValueAsync<DriverService>(this.DriverServiceOverride);
            logger.Information($"DriverService was over-ridden for application : {applicationDetails.ApplicationName}");
            if (!(driverService is T))
            {
                throw new ArgumentException($"Incorrect driver service override : {driverService.GetType()} for browser : {browser}");
            }
            return driverService as T;
        }

        //download the webdriver in to target folder 
        string webDriverFolder = await this.ArgumentProcessor.GetValueAsync<string>(this.WebDriverLocation);
        var shouldDownloadDriver = await this.ArgumentProcessor.GetValueAsync<bool>(this.AutoDownloadDriver);
        if(!shouldDownloadDriver)
        {
            logger.Information("Auto download web driver is not enabled");
            if (!Directory.Exists(webDriverFolder))
            {
                throw new DirectoryNotFoundException($"WebDriver directory {webDriverFolder} doesn't exist.");
            }  
        }
        else
        {
            logger.Information("Auto download web driver is enabled");
            webDriverFolder = SeleniumManager.DriverPath(driverOptions);
        }
        logger.Information("Use webdriver from path : {0}", webDriverFolder);
        //Create a default driver service based on browser type
        switch (browser)
        {
            case Browsers.FireFox:
                driverService = FirefoxDriverService.CreateDefaultService(webDriverFolder);
                break;
            case Browsers.Chrome:
                driverService = ChromeDriverService.CreateDefaultService(webDriverFolder);
                break;
            case Browsers.Edge:
                driverService = EdgeDriverService.CreateDefaultService(webDriverFolder);
                break;
            default:
                throw new ArgumentException("Requested web driver type is not supported");
        }
        return driverService as T;
    }

    /// <summary>
    /// Get the browser <see cref="Process"/>  details launched earlier by WebDriver
    /// </summary>
    /// <param name="preferredBrowser"></param>
    /// <param name="processIdentifier"></param>
    /// <returns></returns>
    private Process GetLaunchedBrowserProcess(Browsers preferredBrowser, string processIdentifier)
    {
        string processName;
        switch (preferredBrowser)
        {
            case Browsers.Chrome:
                processName = "chrome.exe";
                break;
            case Browsers.FireFox:
                processName = "Firefox.exe";
                break;
            case Browsers.Edge:
                processName = "msedge.exe";
                break;
            default:
                throw new ArgumentException("Browser is not supported");
        }

        var processManager = this.EntityManager.GetServiceOfType<IProcessManager>();
        IEnumerable<(int, string)> processDetails = processManager.GetCommandLineArguments(processName);
        foreach (var processDetailsItem in processDetails)
        {
            if (processDetailsItem.Item2.Contains(processIdentifier))
            {
                var process = Process.GetProcessById(processDetailsItem.Item1);
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    return process;
                }
            }
        }
        throw new Exception($"Failed to find launched browser window for : {preferredBrowser} with processIdentifier : {processIdentifier}");
    }
}
