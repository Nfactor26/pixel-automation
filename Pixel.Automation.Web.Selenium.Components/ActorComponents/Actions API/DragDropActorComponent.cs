//using OpenQA.Selenium;
//using OpenQA.Selenium.Interactions;
//using Pixel.Automation.Core;
//using Pixel.Automation.Core.Attributes;
//using Pixel.Automation.Core.Exceptions;
//using System;
//using System.Linq;
//using System.Runtime.Serialization;

//namespace Pixel.Automation.Web.Selenium.Components
//{
//    [DataContract]
//    [Serializable]
//    [ToolBoxItem("Drag Drop", "Selenium","Actions", iconSource: null, description: "Perform a click action on WebElement", tags: new string[] { "Click", "Web" })]

//    public class DragDropActorComponent : SeleniumActorComponent
//    {

//        [RequiredComponent]
//        [System.ComponentModel.Browsable(false)]
//        public WebControlIdentity SouceControlIdentity
//        {
//            get
//            {
//                var controlIdentity = this.Parent.GetComponentsOfType<ControlCompone>().FirstOrDefault(c => c.Tag.Equals("SourceControlIdentity")); ;            
//                return controlIdentity;
//            }
//        }

//        [RequiredComponent]
//        [System.ComponentModel.Browsable(false)]
//        public WebControlIdentity TargetControlIdentity
//        {
//            get
//            {
//                var controlIdentity = this.Parent.GetComponentsOfType<WebControlIdentity>().FirstOrDefault(c => c.Tag.Equals("TargetControlIdentity")); ;
//                return controlIdentity;
//            }
//        }

//        public DragDropActorComponent() : base("Drag Drop", "DragDrop")
//        {

//        }

//        public override void Act()
//        {

//            var sourceControlIdentity = SouceControlIdentity;
//            if (sourceControlIdentity == null)
//                throw new MissingComponentException($"An WebControlIdentityComponent sibling component with tag 'SourceControlIdentity' is required by DragDropActorComponent with id : {Id}");

//            var targetControlIDentity = TargetControlIdentity;
//            if (targetControlIDentity == null)
//                throw new MissingComponentException($"An WebControlIdentityComponent sibling component with tag 'TargetControlIdentity' is required by DragDropActorComponent with id : {Id}");

//            IWebElement sourceControl = SeleniumHelpersComponent.FindControl(sourceControlIdentity);
//            IWebElement targetControl = SeleniumHelpersComponent.FindControl(targetControlIDentity);
//            (new Actions(ApplicationDetails.WebDriver)).DragAndDrop(sourceControl, targetControl).Perform();

//        }

//    }
//}
