using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Switch To Window", "Selenium","Browser", iconSource: null, description: "Switch to desired window using its index", tags: new string[] { "Switch Window", "Web" })]
    public class SwitchToWindowActorComponent : SeleniumActorComponent
    {
        [DataMember(IsRequired = true)]
        [Description("Index (0 based) of the tab/window to be switched to")]     
        public Argument WindowNumber { get; set; } = new InArgument<int>() { DefaultValue = 1 };

        public SwitchToWindowActorComponent():base("Switch To Window","SwitchToWindow")
        {

        }

        public override void Act()
        {
            IWebDriver webDriver = ApplicationDetails.WebDriver;
            int windowNumber = ArgumentProcessor.GetValue<int>(this.WindowNumber);
            if (webDriver.WindowHandles.Count() > windowNumber)
            {
                webDriver.SwitchTo().Window(webDriver.WindowHandles[windowNumber]);
                return;
            }

            throw new IndexOutOfRangeException($"Only {webDriver.WindowHandles.Count} windows / tabs are open. Can't switch to configured window at index : {this.WindowNumber}");
        }


        public override string ToString()
        {
            return "Switch To Window";
        }
    }
}
