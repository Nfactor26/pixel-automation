using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    /// <summary>
    /// Use <see cref="ScrollPageActorComponent"/> to scroll the browser horizontally or vertically or both to sepcified pixels.
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/Window/scroll">scroll</see>
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Scroll Page", "Selenium", iconSource: null, description: "Scroll to element in browser window", tags: new string[] { "Click", "Web" })]

    public class ScrollPageActorComponent : SeleniumActorComponent
    {
        /// <summary>
        /// HorizontalScroll is the pixel along the horizontal axis of the document that you want displayed in the upper left.
        /// </summary>
        [DataMember]
        [Display(Name = "Horizontal Scroll", GroupName = "Configuration", Order = 30, Description = "HorizontalScroll is the pixel along the horizontal" +
            " axis of the document that you want displayed in the upper left.")]       
        public Argument HorizontalScroll { get; set; } = new InArgument<int>() { Mode = ArgumentMode.Default, DefaultValue = 0 };

        /// <summary>
        /// VerticalScroll is the pixel along the vertical axis of the document that you want displayed in the upper left.
        /// </summary>
        [DataMember]  
        [Display(Name = "Vertical Scroll", GroupName = "Configuration", Order = 40, Description = "VerticalScroll is the pixel along the vertical " +
            "axis of the document that you want displayed in the upper left.")]   
        public Argument VerticalScroll { get; set; } = new InArgument<int>() { Mode = ArgumentMode.Default, DefaultValue = 100 };        

        /// <summary>
        /// Default constructor
        /// </summary>
        public ScrollPageActorComponent() : base("Scroll By Amount", "ScrollByAmount")
        {

        }

        /// <summary>
        /// Scroll the browser horizontally or vertically or both.
        /// </summary>
        public override void Act()
        {           
            int verticalScrollAmount = this.ArgumentProcessor.GetValue<int>(this.VerticalScroll);
            int horizontalScrollAmount = this.ArgumentProcessor.GetValue<int>(this.HorizontalScroll);
            _ = ((IJavaScriptExecutor)ApplicationDetails.WebDriver).ExecuteScript($"window.scroll({horizontalScrollAmount}, {verticalScrollAmount});");
        }

        public override string ToString()
        {
            return "Scroll Page Actor";
        }
    }
}
