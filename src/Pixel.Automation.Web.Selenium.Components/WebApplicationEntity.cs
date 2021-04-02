using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Web.Selenium.Components.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Management;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    public class WebApplicationEntity : ApplicationEntity
    {
        private readonly string webDriverFolder = ".//WebDrivers//";

        [Browsable(false)]
        public string Platform { get; set; } = "FireFox";

        /// <summary>
        /// Optional argument which can be used to override the preferred browser configured on application.
        /// </summary>
        [DataMember]
        [Display(Name = "Browser", GroupName = "Overrides", Order = 10, Description = "[Optional] Override the preferred browser option set on Application")]
        public Argument BrowserOverride { get; set; } = new InArgument<Browsers>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        /// <summary>
        /// Optionally specify custom <see cref="DriverOptionsOverride"/> with which WebDriver should be initialized.
        /// If DriverOptions is not configured, default configuration is created and used.
        /// </summary>
        [DataMember]
        [Display(Name = "Driver Options", GroupName = "Overrides", Order = 20, Description = "[Optional] Specify a custom configured driver options." +
            "Default configuration is used if not specified.")]
        public Argument DriverOptionsOverride { get; set; } = new InArgument<DriverOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

        /// <summary>
        /// Optional argument which can be used to override the target url configured on application.
        /// </summary>
        [DataMember]
        [Display(Name = "Target Url", GroupName = "Overrides", Order = 30, Description = "[Optional] Override the Target Url option set on Application")]
        public Argument TargetUriOverride { get; set; } = new InArgument<Uri>() { CanChangeType = false, Mode = ArgumentMode.DataBound };


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
        public override void Launch()
        {
            var webApplicationDetails = this.GetTargetApplicationDetails<WebApplication>();
            if (webApplicationDetails.WebDriver != null)
            {
                logger.Warning($"{webApplicationDetails.ApplicationName} is already launched");
                return;
            }

            string processIdentifier = Guid.NewGuid().ToString();

            Browsers preferredBrowser = webApplicationDetails.PreferredBrowser; 
            if (this.BrowserOverride.IsConfigured())
            {
                preferredBrowser = this.ArgumentProcessor.GetValue<Browsers>(this.BrowserOverride);
                logger.Information($"Preferred browser was over-ridden to {preferredBrowser} for application : {webApplicationDetails.ApplicationName}");
            }
            switch (preferredBrowser)
            {
                case Browsers.FireFox:
                    webApplicationDetails.WebDriver = new FirefoxDriver(webDriverFolder, GetDriverOptions<FirefoxOptions>(preferredBrowser, processIdentifier));
                    break;
                case Browsers.Chrome:
                    webApplicationDetails.WebDriver = new ChromeDriver(webDriverFolder, GetDriverOptions<ChromeOptions>(preferredBrowser, processIdentifier));
                    break;
                case Browsers.Edge:
                    webApplicationDetails.WebDriver = new EdgeDriver(webDriverFolder, GetDriverOptions<EdgeOptions>(preferredBrowser, processIdentifier));
                    break;
                default:
                    throw new ArgumentException("Requested web driver type is not supported");
            }

            logger.Information("{browserToLaunch} has been launched.", preferredBrowser);

            if (webApplicationDetails.MaximizeOnLaunch)
            {
                webApplicationDetails.WebDriver.Manage().Window.Maximize();
            }

            //for firefox/chrome
            var launchedInstance = GetLaunchedBrowserProcess(preferredBrowser, processIdentifier);
            webApplicationDetails.TargetApplication = ApplicationProcess.Attach(launchedInstance);
            logger.Information($"Process Id of launched browser is : {webApplicationDetails.ProcessId}");


            Uri goToUrl = webApplicationDetails.TargetUri;
            if (this.TargetUriOverride.IsConfigured())
            {
                goToUrl = this.ArgumentProcessor.GetValue<Uri>(this.TargetUriOverride);             
                logger.Information($"TargetUri was over-ridden to {goToUrl} for application : {applicationDetails.ApplicationName}");
            }
            webApplicationDetails.WebDriver.Navigate().GoToUrl(goToUrl);

            logger.Information($"{preferredBrowser} has been navigated to {goToUrl}");
        }

        /// <summary>
        /// Close browser by disposing the WebDriver
        /// </summary>
        public override void Close()
        {
            var webApplicationDetails = this.GetTargetApplicationDetails<WebApplication>();
            if (webApplicationDetails.WebDriver != null)
            {
                webApplicationDetails.WebDriver.Quit();
                webApplicationDetails.WebDriver = null;
            }
        }


        /// <summary>
        /// Get DriverOptions for WebDriver based on PreferredBrowser
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="browser"></param>
        /// <param name="processIdentifier"></param>
        /// <returns></returns>
        T GetDriverOptions<T>(Browsers browser, string processIdentifier) where T : DriverOptions
        {
            DriverOptions driverOptions = default;
            if (this.DriverOptionsOverride.IsConfigured())
            {
                driverOptions = this.ArgumentProcessor.GetValue<DriverOptions>(this.DriverOptionsOverride);
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

            string wmiQuery = string.Format("select ProcessId,CommandLine from Win32_Process where Name='{0}'", processName);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            ManagementObjectCollection retObjectCollection = searcher.Get();
            foreach (ManagementObject retObject in retObjectCollection)
            {
                if (retObject["CommandLine"].ToString().Contains(processIdentifier))
                {
                    int processId = Convert.ToInt32(retObject["ProcessId"]);
                    var process = Process.GetProcessById(processId);
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        return process;
                    }
                }
            }
            throw new Exception($"Failed to find launched browser window for : {preferredBrowser} with processIdentifier : {processIdentifier}");
        }
    }
}
