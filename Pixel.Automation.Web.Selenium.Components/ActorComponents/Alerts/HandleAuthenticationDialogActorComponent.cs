//using OpenQA.Selenium;
//using Pixel.Automation.Core.Attributes;
//using System;
//using System.ComponentModel;
//using System.Runtime.Serialization;

//namespace Pixel.Automation.Web.Selenium.Components.Alerts
//{
//    [DataContract]
//    [Serializable]
//    [ToolBoxItem("Handle Authentication", "Selenium", "Alerts", iconSource: null, description: "Set credentials in authentication dialog on web page", tags: new string[] { "Authenticate", "Web" })]

//    public class HandleAuthenticationDialogActorComponent : SeleniumActorComponent
//    {

//        InArgument<string> userName = new InArgument<string>();
//        [DataMember]
//        [Editor(typeof(InArgumentEditor), typeof(InArgumentEditor))]
//        [DisplayName("User Id")]
//        public InArgument<string> UserName
//        {
//            get => userName;
//            set => userName = value;
//        }


//        InArgument<string> password = new InArgument<string>();
//        [DataMember]
//        [Editor(typeof(InArgumentEditor), typeof(InArgumentEditor))]
//        public InArgument<string> Password
//        {
//            get => password;
//            set => password = value;
//        }

//        public HandleAuthenticationDialogActorComponent() : base("Handle Authentication", "HandleAuthenticationDialog")
//        {

//        }

//        public override void Act()
//        {
//            IAlert alert = ApplicationDetails.WebDriver.SwitchTo().Alert();          

//            string userId = ArgumentProcessor.GetValue<string>(this.userName, 0);
//            string password = ArgumentProcessor.GetValue<string>(this.password, 0);

//            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
//                throw new ArgumentNullException($" UserName and Password are mandatory fields");

//            alert.SetAuthenticationCredentials(userId,password);
//        }

//    }
//}
