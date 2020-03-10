using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Double Click", "Selenium", "Actions API", iconSource: null, description: "Perform a double click action on WebElement", tags: new string[] { "DoubleClick", "Web" })]

    public class DoubleClickActorComponent : SeleniumActorComponent
    {
        public DoubleClickActorComponent() : base("Double Click","DoubleClick")
        {

        }

        public override void Act()
        {
            IWebElement control = ControlEntity.GetTargetControl<IWebElement>();
            Actions action = new Actions(ApplicationDetails.WebDriver);
            action.DoubleClick(control).Perform();
        }

        public override string ToString()
        {
            return "Selenium.DoubleClick";
        }
    }
}
