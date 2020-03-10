using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Click And Hold", "Selenium","Actions API", iconSource: null, description: "Perform click and hold on target control", tags: new string[] { "Click", "Hold", "Web" })]
    public class ClickAndHoldActorComponent : SeleniumActorComponent
    {
        public ClickAndHoldActorComponent() : base("Click And Hold", "ClickAndHold")
        {

        }

        public override void Act()
        {
            IWebElement control = ControlEntity.GetTargetControl<IWebElement>();
            Actions action = new Actions(ApplicationDetails.WebDriver);
            action.ClickAndHold(control).Perform();
        }

        public override string ToString()
        {
            return "Selenium.ClickAndHold";
        }
    }
}
