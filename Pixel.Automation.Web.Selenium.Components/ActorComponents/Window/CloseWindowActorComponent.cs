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
    [ToolBoxItem("Close Window", "Selenium","Browser", iconSource: null, description: "Closes a Selenium based browser", tags: new string[] { "Close", "Shutdown", "Dispose", "Web" })]
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
            }
            else
            {
                Log.Warning("Only {n} windows/tabs are open.Can't close configured window with index : {index}", webDriver.WindowHandles.Count(), this.WindowNumber);
            }
        }

        public override bool ValidateComponent()
        {
            if(WindowNumber is InArgument<int> winNumber)
            {
                if(winNumber.DefaultValue < 2)
                {
                    IsValid = true;
                }
            }
            return base.ValidateComponent();
        }

        public override string ToString()
        {
            return "Close Window";
        }
    }
}
