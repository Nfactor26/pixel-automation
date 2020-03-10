using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Right Click", "Selenium", iconSource: null, description: "Perform a right click action on target control", tags: new string[] { "Context Click", "Web" })]
    public class ContextClickActorComponent : SeleniumActorComponent
    {       
        public ContextClickActorComponent() : base("Context Click", "ContextClick")
        {

        }

        public override void Act()
        {
            IWebElement control = ControlEntity.GetTargetControl<IWebElement>();
            Actions action = new Actions(ApplicationDetails.WebDriver);
            action.ContextClick(control).Build().Perform();          
        }

        public override string ToString()
        {
            return "Selenium.RightClick";
        }

    }
}
