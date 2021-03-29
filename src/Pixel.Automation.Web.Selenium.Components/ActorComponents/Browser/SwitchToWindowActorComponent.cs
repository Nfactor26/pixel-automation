﻿using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components.ActorComponents
{
    /// <summary>
    /// Use <see cref="SwitchToWindowActorComponent"/> to switch to a new window or tab to be automated.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Switch To Window", "Selenium", "Browser", iconSource: null, description: "Switch to desired window using its index", tags: new string[] { "Switch Window", "Web" })]
    public class SwitchToWindowActorComponent : SeleniumActorComponent
    {
        private readonly ILogger logger = Log.ForContext<SwitchToWindowActorComponent>();

        /// <summary>
        /// Index of the window or tab to be switched to. Index is 1 based.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Display(Name = "Window Number", GroupName = "Configuration", Order = 10, Description = "Index (1 based) of the window or tab to be switched to")]      
        public Argument WindowNumber { get; set; } = new InArgument<int>() { DefaultValue = 2 };

        /// <summary>
        /// Default constructor
        /// </summary>
        public SwitchToWindowActorComponent():base("Switch To Window", "SwitchToWindow")
        {

        }

        /// <summary>
        /// Switch to the window or tab identifed using the specifed <b>WindowNumber</b>
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Throws IndexOutOfRangeException if the window or tab is not available</exception>
        public override void Act()
        {
            IWebDriver webDriver = ApplicationDetails.WebDriver;
            int windowNumber = ArgumentProcessor.GetValue<int>(this.WindowNumber) - 1;
            if (webDriver.WindowHandles.Count() > windowNumber)
            {
                webDriver.SwitchTo().Window(webDriver.WindowHandles[windowNumber]);
                webDriver.SwitchTo().DefaultContent();
                logger.Information($"WebDriver switched to window/tab number : {windowNumber}");
                return;
            }

            throw new IndexOutOfRangeException($"Only {webDriver.WindowHandles.Count} windows / tabs are open. Can't switch to configured window at index : {this.WindowNumber}");
        }


        public override string ToString()
        {
            return "Switch To Window Actor";
        }
    }
}