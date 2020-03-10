//using Pixel.Automation.Core;
//using Pixel.Automation.Core.Arguments;
//using Pixel.Automation.Core.Interfaces;
//using Pixel.Automation.Core.Models;
//using System.ComponentModel;
//using System.Runtime.Serialization;

//namespace Pixel.Automation.Window.Management
//{
//    public class GetWindowTitleActorComponent : ActorComponent
//    {
//        [DataMember]
//        [DisplayName("Target Window")]
//        [Category("Input")]
//        public Argument ApplicationWindow { get; set; } = new InArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };

//        [DataMember]
//        [DisplayName("Title")]
//        [Category("Input")]
//        [Description("Title of the window")]
//        public Argument Title { get; set; } = new OutArgument<string>() { CanChangeType = false };

//        public GetWindowTitleActorComponent() : base("Get Window Title", "GetWindowTitle")
//        {

//        }

//        public override void Act()
//        {
//            IArgumentProcessor argumentProcessor = this.EntityManager.GetServiceOfType<IArgumentProcessor>();
//            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

//            var targetWindow = argumentProcessor.GetValue<ApplicationWindow>(this.ApplicationWindow);
//            var windowTitle = windowManager.GetWindowTitle(targetWindow);

//            argumentProcessor.SetValue<string>(this.Title, windowTitle);
//        }
//    }
//}
