using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Close Window", "Selenium","Browser", iconSource: null, description: "Close a tab or window", tags: new string[] { "Close", "Tab" , "Window", "Web" })]
    public class CloseWindowActorComponent : SeleniumActorComponent
    {
        [DataMember]
        [Description("Index (1 based) of the tab/window to be closed.Default tab/window can't be closed")]
        public Argument WindowNumber { get; set; } = new InArgument<int>() { DefaultValue = 2 };

        public CloseWindowActorComponent() : base("Close Window", "CloseWindow")
        {           
        }

        public override void Act()
        {
            IWebDriver webDriver = ApplicationDetails.WebDriver;
            int windowNumber = ArgumentProcessor.GetValue<int>(this.WindowNumber);
            if (webDriver.WindowHandles.Count() >= windowNumber)
            {
                webDriver.SwitchTo().Window(webDriver.WindowHandles[windowNumber-1]);
                webDriver.Close();
                return;
            }

            throw new IndexOutOfRangeException($"Only {webDriver.WindowHandles.Count} windows / tabs are open. Can't close configured window  with index : {this.WindowNumber}");
        }     

        public override string ToString()
        {
            return "Close Window";
        }
    }
}
