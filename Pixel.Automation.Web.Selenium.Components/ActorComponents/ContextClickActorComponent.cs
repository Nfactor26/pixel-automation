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
    [ToolBoxItem("Right Click", "Selenium", iconSource: null, description: "Perform a right click action on target control", tags: new string[] { "Context Click", "Web" })]
    public class ContextClickActorComponent : SeleniumActorComponent
    {
        [DataMember]
        [DisplayName("Target Control")]
        [Category("Control Details")]
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        public ContextClickActorComponent() : base("Context Click", "ContextClick")
        {

        }

        public override void Act()
        {
            IWebElement control = GetTargetControl(this.TargetControl);
            Actions action = new Actions(ApplicationDetails.WebDriver);
            action.ContextClick(control).Build().Perform();          
        }

        public override string ToString()
        {
            return "Selenium.RightClick";
        }

    }
}
