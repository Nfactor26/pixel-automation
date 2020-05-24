using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Mouse Over", "Selenium","Actions API", iconSource: null, description: "Perform a mouse over action on WebElement", tags: new string[] { "MouseOver", "Web" })]

    public class MoveToElementActorComponent : SeleniumActorComponent
    {
        public MoveToElementActorComponent() : base("Mouse Over","MouseOver")
        {

        }

        public override void Act()
        {
            IWebElement control = ControlEntity.GetTargetControl<IWebElement>();
            Actions action = new Actions(ApplicationDetails.WebDriver);
            action.MoveToElement(control).Perform();
        }

        public override string ToString()
        {
            return "Selenium.MouseOver";
        }
    }
}
