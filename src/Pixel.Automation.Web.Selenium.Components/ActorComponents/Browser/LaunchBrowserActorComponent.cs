using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Web.Selenium.Components.Enums;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Management;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    /// <summary>
    /// Use <see cref="LaunchBrowserActorComponent"/> to launch a browser using selenium webdriver.
    /// Browser details are specified using <see cref="WebApplication"/>. 
    /// Driver executables should be present in .\\WebDrivers directory
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Launch Browser", "Application", "Browser", iconSource: null, description: "Launch a Selenium based browser", tags: new string[] { "Launch", "Web" })]
    public class LaunchBrowserActorComponent : SeleniumActorComponent
    {
        private readonly ILogger logger = Log.ForContext<LaunchBrowserActorComponent>();
        private readonly string webDriverFolder = ".//WebDrivers//";

        /// <summary>
        /// Optional argument which can be used to override the preferred browser configured on application.
        /// </summary>
        [DataMember]
        [Display(Name = "Browser Override", GroupName = "Configuration", Order = 10, Description = "[Optional] Override the preferred browser option set on Application")]
        public Argument BrowserOverride { get; set; } = new InArgument<Browsers>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        /// <summary>
        /// Optionally specify custom <see cref="DriverOptions"/> with which WebDriver should be initialized.
        /// If DriverOptions is not configured, default configuration is created and used.
        /// </summary>
        [DataMember]
        [Display(Name = "Driver Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Specify a custom configured driver options." +
            "Default configuration is used if not specified.")]
        public Argument DriverOptions { get; set; } = new InArgument<DriverOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

        /// <summary>
        /// Indicates whether the browser should be maximized on launch. Default value is true.
        /// </summary>
        [DataMember]
        [Display(Name = "Maximize browser", GroupName = "Configuration", Order = 20)]
        [Description("Specify whether browser window should be maximized after launch.")]
        public bool MaximizeOnLaunch { get; set; } = true;

        /// <summary>
        /// Default constructor
        /// </summary>
        public LaunchBrowserActorComponent():base("Launch Browser", "LaunchBrowser")
        {            
          
        }      
        
        /// <summary>
        /// Launch the configured browser for <see cref="WebApplication"/> using selenium webdriver.
        /// Custom <see cref="DriverOptions"/> can be specified. 
        /// </summary>
        public override void Act()
        {
            var applicationDetails = ApplicationDetails;           
            string processIdentifier = Guid.NewGuid().ToString();

            IWebDriver webDriver;
            var browserToLaunch = applicationDetails.PreferredBrowser;
            if(this.BrowserOverride.IsConfigured())
            {
                browserToLaunch = this.ArgumentProcessor.GetValue<Browsers>(this.BrowserOverride);
                logger.Information("Preferred browser was over-ridden to {browserToLaunch}", browserToLaunch);
            }
            switch (browserToLaunch)
            {
                case Browsers.FireFox:                  
                    webDriver = new FirefoxDriver(webDriverFolder, GetDriverOptions<FirefoxOptions>(browserToLaunch, processIdentifier));
                    break;
                case Browsers.Chrome:                    
                    webDriver = new ChromeDriver(webDriverFolder, GetDriverOptions<ChromeOptions>(browserToLaunch, processIdentifier));
                    break;
                case Browsers.Edge:                  
                    webDriver = new EdgeDriver(webDriverFolder, GetDriverOptions<EdgeOptions>(browserToLaunch, processIdentifier));
                    break;
                default:
                    throw new ArgumentException("Requested web driver type is not supported");
            }

            logger.Information("{browserToLaunch} has been launched.", browserToLaunch);

            if(this.MaximizeOnLaunch)
            {
                webDriver.Manage().Window.Maximize();
            }
            ApplicationDetails.WebDriver = webDriver;

            //for firefox/chrome
            Process launchedInstance = GetLaunchedBrowserProcess(browserToLaunch, processIdentifier);
            if(launchedInstance!=null)
            {
                applicationDetails.SetProcessDetails(launchedInstance);
                logger.Information($"Process Id of launched browser is : {applicationDetails.ProcessId}");
            }
            
             Uri targetUrl = applicationDetails.TargetUri;
             webDriver.Navigate().GoToUrl(targetUrl);

            logger.Information($"{browserToLaunch} has been navigated to {applicationDetails.TargetUri}");
        }


        T GetDriverOptions<T>(Browsers browser, string processIdentifier) where  T : DriverOptions
        {
            DriverOptions driverOptions = null;
            if (this.DriverOptions.IsConfigured())
            {
                driverOptions = ArgumentProcessor.GetValue<DriverOptions>(this.DriverOptions);
                if(!(driverOptions is T t))
                {
                    throw new ArgumentException($"Incorrect driver option : {driverOptions.GetType()} for browser : {browser}");
                }
            }

            switch (browser)
            {
                case Browsers.FireFox:
                    FirefoxOptions firefoxOptions = driverOptions as FirefoxOptions ?? new FirefoxOptions();
                    firefoxOptions.AddArgument($"--{processIdentifier}");
                    return firefoxOptions as T;
                case Browsers.Chrome:
                    ChromeOptions chromeOptions = driverOptions as ChromeOptions ?? new ChromeOptions();   
                    //TODO : Check what "test-type" parameter does and add a comment here.
                    chromeOptions.AddArgument("test-type");
                    chromeOptions.AddArgument(processIdentifier);
                    return chromeOptions as T;
                case Browsers.Edge:
                    EdgeOptions edgeOptions = driverOptions as EdgeOptions ?? new EdgeOptions();
                    edgeOptions.UseChromium = true;
                    //edgeOptions.AddArgument("test-type");
                    edgeOptions.AddArgument($"--{processIdentifier}");
                    return edgeOptions as T;
                default:
                    throw new ArgumentException("Requested web driver type is not supported");
            }         
        }

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

            string wmiQuery = string.Format("select ProcessId,CommandLine from Win32_Process where Name='{0}'", processName);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection retObjectCollection = searcher.Get();
            foreach (ManagementObject retObject in retObjectCollection)
            {
                if (retObject["CommandLine"].ToString().Contains(processIdentifier))
                {
                    int processId = Convert.ToInt32(retObject["ProcessId"]);                  
                    var process = Process.GetProcessById(processId);
                    if(process.MainWindowHandle != IntPtr.Zero)
                    {
                        return process;
                    }
                }
            }
            throw new Exception($"Failed to find launched browser window for : {preferredBrowser} with processIdentifier : {processIdentifier}");
        }

        public override string ToString()
        {
            return "Launch Browser Actor";
        }
    }
}
