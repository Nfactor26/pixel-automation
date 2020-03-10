using OpenQA.Selenium;
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

        public ClickActorComponent():base("Click","Click")
        {

        }

        public override void Act()
        {
            UIControl targetControl;
            if (this.TargetControl.IsConfigured())
            {              
                targetControl = ArgumentProcessor.GetValue<UIControl>(this.TargetControl);               
            }
            else
            {
                ThrowIfMissingControlEntity();
                targetControl = this.ControlEntity.GetControl();              
            }

            IWebElement control = targetControl.GetApiControl<IWebElement>(); 
            control.Click();
            Log.Information("Click interaction completed");
        }

        public override string ToString()
        {
            return "Selenium.Click";
        }
    }
}
