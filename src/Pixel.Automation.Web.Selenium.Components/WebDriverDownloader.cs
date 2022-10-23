
using Dawn;
using Pixel.Automation.Web.Selenium.Components.Enums;
using Serilog;
using WebDriverManager;
using WebDriverManager.DriverConfigs;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace Pixel.Automation.Web.Selenium.Components;

public class WebDriverDownloader
{
    private readonly ILogger logger = Log.ForContext<WebDriverDownloader>();
    private readonly string downloadsDirectory;
    private readonly Browsers forBrowser;
    private readonly IDriverConfig driverConfig;

    /// <summary>
    /// constructor
    /// <param name="downloadsDirectory">Root directory for the webdriver</param>
    /// <param name="forBrowser">Target browser</param>
    /// </summary>   
    public WebDriverDownloader(string downloadsDirectory, Browsers forBrowser)
    {
        this.downloadsDirectory = Guard.Argument(downloadsDirectory, nameof(downloadsDirectory)).NotNull().NotEmpty().NotWhiteSpace();
        this.forBrowser = forBrowser;
        this.driverConfig = GetDriverConfig();
    }

    /// <summary>
    /// Download latest version of the webdriver
    /// </summary>
    public string DownloadLatestVersion()
    {
        var latestVersion = this.driverConfig.GetLatestVersion();
        string downloadLocation = Path.Combine(downloadsDirectory, this.driverConfig.GetName(), latestVersion,
        ArchitectureHelper.GetArchitecture().ToString(), this.driverConfig.GetBinaryName());
        if (!File.Exists(downloadLocation))
        {
            var driverManager = new DriverManager(downloadsDirectory);
            driverManager.SetUpDriver(driverConfig, VersionResolveStrategy.Latest);
            logger.Information("Download latest version {0} of {1}", latestVersion, driverConfig.GetBinaryName());
        }
        return Path.GetDirectoryName(downloadLocation);
    }
    
    /// <summary>
    /// Download matching version of the web driver based on installed browser version
    /// </summary>
    public string DownloadMatchingVersion()
    {
        var matchingVersion = this.driverConfig.GetMatchingBrowserVersion();
        string downloadLocation = Path.Combine(downloadsDirectory, this.driverConfig.GetName(), matchingVersion, ArchitectureHelper.GetArchitecture().ToString(), this.driverConfig.GetBinaryName());
        if (!File.Exists(downloadLocation))
        {
            var driverManager = new DriverManager(downloadsDirectory);
            driverManager.SetUpDriver(this.driverConfig, VersionResolveStrategy.MatchingBrowser);
            logger.Information("Download matching version {0} of {1}", matchingVersion, driverConfig.GetBinaryName());
        }
        return Path.GetDirectoryName(downloadLocation);
    }

    private IDriverConfig GetDriverConfig()
    {
        switch (forBrowser)
        {
            case Browsers.Chrome:
                return new ChromeConfig();
            case Browsers.FireFox:
                return new FirefoxConfig();
            case Browsers.Edge:
                return new EdgeConfig();
            default:
                throw new ArgumentException($"{forBrowser} is not a supported browser");
        }
    }

}
