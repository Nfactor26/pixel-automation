using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    /// <summary>
    /// Use <see cref="CloseBrowserActorComponent"/> to close a browser window that was started by <see cref="LaunchBrowserActorComponent"/> using selenium webdriver.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Close Browser", "Selenium", "Browser", iconSource: null, description: "Closes a Selenium based browser", tags: new string[] { "Close", "Shutdown", "Dispose", "Web" })]
    public class CloseBrowserActorComponent : SeleniumActorComponent
    {
        private readonly ILogger logger = Log.ForContext<CloseBrowserActorComponent>();
      
        /// <summary>
        /// Default constructor
        /// </summary>
        public CloseBrowserActorComponent():base("Close Browser", "CloseBrowser")
        {
            
        }

        /// <summary>
        /// Close the browser window and dispose the selenium webdriver
        /// </summary>
        public override void Act()
        {
            ApplicationDetails.Dispose();
            logger.Information($"Browser for application : {ApplicationDetails.ApplicationName} was closed.");
        }

        public override string ToString()
        {
            return "Close Browser Actor";
        }
    }
}
