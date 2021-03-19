using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    /// <summary>
    /// Use <see cref="NavigateActorComponent"/> to navigate the browser to a specified url.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Navigate To", "Selenium", "Browser", iconSource: null, description: "Navigate the browser to desired url", tags: new string[] { "Navigate", "Web" })]
    public class NavigateActorComponent : SeleniumActorComponent
    {
        private readonly ILogger logger = Log.ForContext<NavigateActorComponent>();

        /// <summary>
        /// Url to which active window/tab should be navigated
        /// </summary>
        [DataMember(IsRequired = true)]   
        [Display(Name = "Url", GroupName = "Configuration", Order = 20, Description = "Url to navigate to")]        
        public Argument TargetUrl { get; set; } = new InArgument<Uri>() { DefaultValue = new Uri("https://www.bing.com") };

        /// <summary>
        /// Default constructor
        /// </summary>
        public NavigateActorComponent():base("Navigate","Navigate")
        {

        }

        /// <summary>
        /// Navigate the active window/tab of the brower to a configured url.
        /// </summary>
        public override void Act()
        {
            Uri targetUrl = ArgumentProcessor.GetValue<Uri>(this.TargetUrl);
            this.ApplicationDetails.WebDriver.Navigate().GoToUrl(targetUrl);
            Log.Information($"Navigated to Url : {targetUrl}");
        }

        public override string ToString()
        {
            return "Navigate Actor";
        }
    }
}
