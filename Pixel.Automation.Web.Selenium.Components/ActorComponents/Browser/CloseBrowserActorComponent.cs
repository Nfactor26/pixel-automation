using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Close Browser", "Selenium","Browser", iconSource: null, description: "Closes a Selenium based browser", tags: new string[] { "Close","Shutdown","Dispose", "Web" })]
    public class CloseBrowserActorComponent : SeleniumActorComponent
    {
        public override void Act()
        {
            ApplicationDetails.Dispose();
        }

        public CloseBrowserActorComponent():base("Close Browser", "CloseBrowser")
        {
            
        }

        public override string ToString()
        {
            return "Close Browser";
        }
    }
}
