using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Actions Click", "Selenium", "Actions API", iconSource: null, description: "Perform a click action on WebElement. Unlike Click, it doesn't check for preconditions like whether element is clickable,etc", tags: new string[] { "Click", "Web" })]
    public class ActionsClickActorComponent : SeleniumActorComponent
    {
        public ActionsClickActorComponent() : base("ActionsClick", "ActionsClick")
        {

        }

        public override void Act()
        {
            IWebElement control = ControlEntity.GetTargetControl<IWebElement>();
            Actions action = new Actions(ApplicationDetails.WebDriver);
            action.Click(control).Build().Perform();

            Log.Information("Click performed on  Control using Actions API: {$ControlIdentity}", ControlEntity.ControlDetails);
        }

        public override string ToString()
        {
            return "Selenium.Actions.Click";
        }
    }
}
