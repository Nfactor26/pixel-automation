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
    [ToolBoxItem("Scroll To Element", "Selenium", iconSource: null, description: "Scroll to element in browser window", tags: new string[] { "Click", "Web" })]

    public class ScrollToActorComponent : SeleniumActorComponent
    {
        [DisplayName("Target Control")]
        [Category("Control Details")]            
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };


        [DataMember]      
        [Description("Amount of offset from element's actual y coordinate to apply while scrolling")]
        public Argument OffSet { get; set; } = new InArgument<int>() { DefaultValue = 0 };

        public ScrollToActorComponent() : base("Scroll To Element", "ScrollToElement")
        {

        }

        public override void Act()
        {
            UIControl targetControl = default;
            if (this.TargetControl.IsConfigured())
            {
                targetControl = ArgumentProcessor.GetValue<UIControl>(this.TargetControl);
            }
            else
            {
                ThrowIfMissingControlEntity();
                targetControl = this.ControlEntity.GetControl();
            }

            int offsetAmount = ArgumentProcessor.GetValue<int>(this.OffSet);
            IWebElement control = targetControl.GetApiControl<IWebElement>();
            int elemPos = control.Location.Y+ offsetAmount;
            ((IJavaScriptExecutor)ApplicationDetails.WebDriver).ExecuteScript("window.scroll(0, " + elemPos + ");");

            Log.Information("ScrollTo interaction completed");
        }

        public override string ToString()
        {
            return "Scroll To Element";
        }
    }
}
