using OpenQA.Selenium;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components.Alerts
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Handle Alert", "Selenium","Alerts", iconSource: null, description: "Accept/Dissmiss alert on web page", tags: new string[] { "Click", "Web" })]
    public class HandleAlertActorComponent : SeleniumActorComponent
    {
        [DataMember]
        public HandleAlertBehavior Action { get; set; }

        public HandleAlertActorComponent() : base("Handle Alert", "HandleAlert")
        {

        }

        public override void Act()
        {
            IAlert alert = ApplicationDetails.WebDriver.SwitchTo().Alert();
            switch(this.Action)
            {
                case HandleAlertBehavior.Accept:
                    alert.Accept();
                    break;
                case HandleAlertBehavior.Dismiss:
                    alert.Dismiss();
                    break;
            }          
        }

        public override string ToString()
        {
            return "Selenium.HandleAlert";
        }
    }    
}
