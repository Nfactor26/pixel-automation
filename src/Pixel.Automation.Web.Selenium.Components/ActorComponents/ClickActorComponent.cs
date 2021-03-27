using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    /// <summary>
    /// Use <see cref="ClickActorComponent"/> to perform a click operation on web control using selenium webdriver.  
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Click", "Selenium", iconSource: null, description: "Perform a click action on WebElement", tags: new string[] { "Click", "Web" })]
    public class ClickActorComponent : WebElementActorComponent
    {
        private readonly ILogger logger = Log.ForContext<ClickActorComponent>();
             
        [DataMember]
        [Display(Name = "Force click", GroupName = "Configuration", Order = 20, Description = "When true, use actions api to perform click" +
            "which doesn't validate if control is enabled, visible, etc.")]     
        public bool ForceClick { get; set; } = false;

        public ClickActorComponent() : base("Click", "Click")
        {

        }

        /// <summary>
        /// Perform a click on <see cref="IWebElement"/> using Click() method.
        /// However, if force click is configured, actions api is used instead which doesn't throw 
        /// exception if element is disabled, hidden, beneath other control, etc.
        /// </summary>
        public override void Act()
        {
            IWebElement control = GetTargetControl();
            if (this.ForceClick)
            {
                //doesn't check for preconditions like whether element is clickable,etc
                Actions action = new Actions(ApplicationDetails.WebDriver);
                action.Click(control).Build().Perform();
                logger.Information("control was force clicked.");
            }
            else
            {
                control.Click();
                logger.Information("control was clicked.");
            }
        }

        public override string ToString()
        {
            return "Click Actor";
        }
    }
}
