using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Scroll Page", "Selenium", iconSource: null, description: "Scroll to element in browser window", tags: new string[] { "Click", "Web" })]

    public class ScrollPageActorComponent : SeleniumActorComponent
    {
        [DataMember]
        [Description("Amount of horizontal scroll")]
        public Argument HorizontalScroll { get; set; } = new InArgument<int>() { Mode = ArgumentMode.Default, DefaultValue = 0 };

        [DataMember]              
        [Description("Amount of vertical scroll")]
        public Argument VerticalScroll { get; set; } = new InArgument<int>() { Mode = ArgumentMode.Default, DefaultValue = 0 };        

        public ScrollPageActorComponent() : base("Scroll By Amount", "ScrollByAmount")
        {

        }

        public override void Act()
        {           
            int verticalScrollAmount = this.ArgumentProcessor.GetValue<int>(this.VerticalScroll);
            int horizontalScrollAmount = this.ArgumentProcessor.GetValue<int>(this.HorizontalScroll);
            _ = ((IJavaScriptExecutor)ApplicationDetails.WebDriver).ExecuteScript($"window.scroll({horizontalScrollAmount}, {verticalScrollAmount});");
        }

        public override string ToString()
        {
            return "Scroll Page";
        }
    }
}
