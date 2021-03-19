using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading;

namespace Pixel.Automation.Web.Selenium.Components.Alerts
{
    /// <summary>
    /// Use <see cref="HandlePromptActorComponent"/> to interact with a browser prompt window
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Handle Prompt", "Selenium", "Alerts", iconSource: null, description: "Interact with browser prompot window", tags: new string[] { "prompt", "Web" })]
    public class HandlePromptActorComponent : SeleniumActorComponent
    {
        private readonly ILogger logger = Log.ForContext<HandlePromptActorComponent>();

        HandleAlertBehavior action;
        /// <summary>
        /// Indicates whether to accept or dismiss prompt
        /// </summary>
        [DataMember]
        [Display(Name = "Action", GroupName = "Configuration", Order = 10, Description = "Indicates whether to accept or dismiss prompt")] 
        public HandleAlertBehavior Action
        {
            get
            {
                if (this.action == HandleAlertBehavior.Dismiss)
                {
                    this.SetDispalyAttribute(nameof(Message), false);
                }
                else
                {
                    this.SetDispalyAttribute(nameof(Message), true);

                }
                return action;
            }
            set
            {
                action = value;
            }
        }

        /// <summary>
        /// Input value for prompt dialog
        /// </summary>
        [DataMember]
        [Display(Name = "Prompt Value", GroupName = "Configuration", Order = 20, Description = "Input for prompt")]     
        [Browsable(false)]
        [RefreshProperties(RefreshProperties.Repaint)]
        public Argument Message { get; set; } = new InArgument<string>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        /// <summary>
        /// Default constructor
        /// </summary>
        public HandlePromptActorComponent() : base("Handle Prompt", "HandlePrompt")
        {

        }

        /// <summary>
        /// Accept a browser prompt by providing an input value or dismiss the prompt.s
        /// </summary>
        public override void Act()
        {
            IAlert alert = ApplicationDetails.WebDriver.SwitchTo().Alert();
            switch (this.action)
            {
                case HandleAlertBehavior.Accept:
                    string input = ArgumentProcessor.GetValue<string>(this.Message);
                    alert.SendKeys(input);
                    Thread.Sleep(500);
                    alert.Accept();
                    logger.Information("Accepted browser prompt by providing configured prompt value");
                    break;
                case HandleAlertBehavior.Dismiss:
                    alert.Dismiss();
                    logger.Information("Browser prompt was dismissed");
                    break;
            }
        }

        public override string ToString()
        {
            return "Handle Prompt Actor";
        }

    }
}
