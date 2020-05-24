using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;


namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Click", "Selenium", iconSource: null, description: "Perform a click action on WebElement", tags: new string[] { "Click", "Web" })]
    public class ClickActorComponent : SeleniumActorComponent
    {
        [DataMember]
        [DisplayName("Target Control")]
        [Category("Control Details")]          
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        [DataMember]
        [DisplayName("Force Click")]
        public bool ForceClick { get; set; } = false;

        public ClickActorComponent():base("Click","Click")
        {

        }

        public override void Act()
        {
            IWebElement control = GetTargetControl(this.TargetControl);
            if(this.ForceClick)
            {
                //doesn't check for preconditions like whether element is clickable,etc
                Actions action = new Actions(ApplicationDetails.WebDriver);
                action.Click(control).Build().Perform();
            }
            else
            {
                control.Click();
            }
           
            Log.Information("Click completed");
        }

        public override string ToString()
        {
            return "Selenium.Click";
        }
    }
}
