//using OpenQA.Selenium;
//using Pixel.Automation.Core;
//using Pixel.Automation.Core.Attributes;
//using Pixel.Automation.Core.Enums;
//using System;
//using System.ComponentModel;
//using System.Runtime.Serialization;

//namespace Pixel.Automation.Web.Selenium.Components.Alerts
//{
//    [DataContract]
//    [Serializable]
//    [ToolBoxItem("Handle Prompt", "Selenium", "Alerts", iconSource: null, description: "SendKeys to prompt dialog on web page", tags: new string[] { "prompt", "Web" })]

//    public class PromptDialogHandlerActorComponent : SeleniumActorComponent
//    {
//        HandleAlertBehavior action;
//        [DataMember]
//        [DisplayName("Action")]
//        public HandleAlertBehavior Action
//        {
//            get
//            {
//                return action;
//            }
//            set
//            {
//                action = value;
//                if (value == HandleAlertBehavior.Dismiss)
//                {
//                    this.SetBrowsableAttribute("Input", false);
//                }
//                else
//                {
//                    this.SetBrowsableAttribute("Input", true);

//                }
//            }
//        }

//        InArgument<string> input = new InArgument<string>();
//        [DataMember]
//        [Editor(typeof(InArgumentEditor), typeof(InArgumentEditor))]
//        [DisplayName("Prompt Message")]
//        [Browsable(false)]
//        public InArgument<string> Input
//        {
//            get => input;
//            set => input = value;
//        }

//        public PromptDialogHandlerActorComponent() : base("Handle Prompot", "HandlePrompt")
//        {

//        }

//        public override void Act()
//        {
//            IAlert alert = ApplicationDetails.WebDriver.SwitchTo().Alert();
//            switch (this.action)
//            {
//                case HandleAlertBehavior.Accept:
//                    string input = ArgumentProcessor.GetValue<string>(this.input, 0);
//                    alert.SendKeys(input);
//                    alert.Accept();
//                    break;
//                case HandleAlertBehavior.Dismiss:
//                    alert.Dismiss();
//                    break;
//            }
//        }


//    }
//}
