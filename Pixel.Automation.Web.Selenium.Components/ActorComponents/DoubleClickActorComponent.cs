using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Double Click", "Selenium", "Actions API", iconSource: null, description: "Perform a double click action on WebElement", tags: new string[] { "DoubleClick", "Web" })]

    public class DoubleClickActorComponent : SeleniumActorComponent
    {
        [DataMember]
        [DisplayName("Target Control")]
        [Category("Control Details")]
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        public DoubleClickActorComponent() : base("Double Click","DoubleClick")
        {

        }

        public override void Act()
        {
            IWebElement control = GetTargetControl(this.TargetControl);
            Actions action = new Actions(ApplicationDetails.WebDriver);
            action.DoubleClick(control).Perform();
        }

        public override string ToString()
        {
            return "Selenium.DoubleClick";
        }
    }
}
