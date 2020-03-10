using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Opera;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Exceptions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Launch Browser", "Selenium", "Browser", iconSource: null, description: "Launch a Selenium based browser", tags: new string[] { "Launch", "Web" })]
    public class LaunchBrowserActorComponent : SeleniumActorComponent
    {
        private readonly string webDriverFolder = ".//WebDrivers//";

        public LaunchBrowserActorComponent():base("Launch Browser","LaunchBrowser")
        {            
            this.ProcessOrder = 0;
        }      
        
        public override void Act()
        {
            var applicationDetails = ApplicationDetails;           
            if (applicationDetails == null)
            {
                throw new MissingComponentException("Web Application Details component is missing.Launch will fail.");
            }

            IWebDriver webDriver = null;
            string processIdentifier = Guid.NewGuid().ToString();
            switch (applicationDetails.PreferredBrowser)
            {
                case Browsers.FireFox:
                    FirefoxOptions firefoxOptions = new FirefoxOptions();
                    firefoxOptions.AddArgument($"--{processIdentifier}");
                    webDriver = new FirefoxDriver(webDriverFolder,firefoxOptions);                    
                    break;
                case Browsers.Chrome:
                    ChromeOptions chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("test-type");                    
                    chromeOptions.AddArgument(processIdentifier);
                    webDriver = new ChromeDriver(webDriverFolder, chromeOptions);                  
                    break;
                case Browsers.Opera:
                    OperaOptions operaOptions = new OperaOptions();
                    operaOptions.AddArgument($"--{processIdentifier}");
                    operaOptions.AddArgument("test-type");                 
                    operaOptions.BinaryLocation = @"C:\Users\Nish26\AppData\Local\Programs\Opera\44.0.2510.1159\opera.exe";
                    webDriver = new OperaDriver(webDriverFolder, operaOptions);
                    break;              
                case Browsers.InternetExplorer:

                    //Note : We could just query if the commandline contains targeturl/loginurl . However, many a times browsers get redirected to some other url
                    List<Process> previouslyRunningIEInstances = new List<Process>();
                    List<Process> launchedIEInstances = new List<Process>();

                    string wmiQuery ="select ProcessId,CommandLine from Win32_Process where Name='iexplore.exe'";
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
                    ManagementObjectCollection retObjectCollection = searcher.Get();
                    foreach (ManagementObject retObject in retObjectCollection)
                    {
                        if (retObject["CommandLine"].ToString().Contains("noframemerging"))
                        {
                            int processId = Convert.ToInt32(retObject["ProcessId"]);
                            previouslyRunningIEInstances.Add(Process.GetProcessById(processId));
                        }
                    }
                                      
                    InternetExplorerOptions ieOptions = new InternetExplorerOptions();
                    //ieOptions.BrowserCommandLineArguments = $"--{processIdentifier}";
                    ieOptions.InitialBrowserUrl = applicationDetails.TargetUri.ToString();        
                    webDriver = new InternetExplorerDriver(ieOptions);
                                     
                    retObjectCollection = searcher.Get();
                    foreach (ManagementObject retObject in retObjectCollection)
                    {
                        if (retObject["CommandLine"].ToString().Contains("noframemerging"))
                        {
                            int processId = Convert.ToInt32(retObject["ProcessId"]);
                            if(!previouslyRunningIEInstances.Any(p=>p.Id.Equals(processId)))
                                launchedIEInstances.Add(Process.GetProcessById(processId));
                        }
                    }                
                    if (launchedIEInstances.Count() != 1)
                    {
                        Log.Warning("More than one or No iexplore.exe have been started.It is not possible to determine target process started by IEDriver");
                    }
                    else
                    {
                        applicationDetails.SetProcessDetails(launchedIEInstances.First());
                        Log.Information("{PreferredBrowser} has been launched with process id {ProcessId}", applicationDetails.PreferredBrowser, applicationDetails.ProcessId);
                    }
                    break;

                default:
                    throw new ArgumentException("Requested web driver type is not supported");
            }

            Log.Information("{PreferredBrowser} has been launched.", applicationDetails.PreferredBrowser);

            webDriver.Manage().Window.Maximize();
            ApplicationDetails.WebDriver = webDriver;

            //for firefox/chrome/opera
            Process launchedInstance = GetLaunchedBrowserProcess(applicationDetails.PreferredBrowser, processIdentifier);
            if(launchedInstance!=null)
            {
                applicationDetails.SetProcessDetails(launchedInstance);
                Log.Information("{PreferredBrowser} has been launched with process id {ProcessId}", applicationDetails.PreferredBrowser, applicationDetails.ProcessId);
            }
            
             Uri targetUrl = applicationDetails.TargetUri;
             webDriver.Navigate().GoToUrl(targetUrl);

            Log.Information("{PreferredBrowser} has been navigated to {Url}", applicationDetails.PreferredBrowser, applicationDetails.TargetUri);
        }

        private Process GetLaunchedBrowserProcess(Browsers preferredBrowser,string processIdentifier)
        {
            string processName = string.Empty;
            switch(preferredBrowser)
            {
                case Browsers.Chrome:
                    processName = "chrome.exe";
                    break;
                case Browsers.FireFox:
                    processName = "Firefox.exe";
                    break;
                case Browsers.Opera:
                    processName = "opera.exe";
                    break;
                case Browsers.InternetExplorer:
                    return null; //Ie driver doesn't have option to add argument :s                   
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
            return "Launch Browser";
        }
    }
}
