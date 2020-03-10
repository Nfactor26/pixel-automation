using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Navigate To", "Selenium", "Browser", iconSource: null, description: "Navigate the browser to desired url", tags: new string[] { "Navigate", "Web" })]
    public class NavigateActorComponent : SeleniumActorComponent
    {
        [DataMember(IsRequired = true)]        
        [Description("Url to navigate to")]
        public Argument TargetUrl { get; set; } = new InArgument<Uri>() { DefaultValue = new Uri("https://www.bing.com") };

        public NavigateActorComponent():base("Navigate","Navigate")
        {

        }

        public override void Act()
        {
            Uri targetUrl = ArgumentProcessor.GetValue<Uri>(this.TargetUrl);
            this.ApplicationDetails.WebDriver.Navigate().GoToUrl(targetUrl);
            Log.Information("Navigated to Uri : {targetUrl}");
        }

        public override string ToString()
        {
            return "Navigate To";
        }
    }
}
