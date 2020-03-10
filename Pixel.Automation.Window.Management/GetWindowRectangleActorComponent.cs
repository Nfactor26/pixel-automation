//using Pixel.Automation.Core;
//using Pixel.Automation.Core.Arguments;
//using Pixel.Automation.Core.Attributes;
//using Pixel.Automation.Core.Devices;
//using Pixel.Automation.Core.Interfaces;
//using Pixel.Automation.Core.Models;
//using System;
//using System.ComponentModel;
//using System.Drawing;
//using System.Runtime.Serialization;

//namespace Pixel.Automation.Window.Management
//{
//    /// <summary>
//    /// Move the window to a new position
//    /// </summary>
//    [DataContract]
//    [Serializable]
//    [ToolBoxItem("Get Window Rect", "Window Managment", iconSource: null, description: "Get window Rectangle i.e. it's position on screen and it's dimension", tags: new string[] { "Size" ,"Rectangle" })]
//    public class GetWindowRectangleActorComponent : ActorComponent
//    {
//        [DataMember]
//        [DisplayName("Target Window")]
//        [Category("Input")]
//        public Argument ApplicationWindow { get; set; } = new InArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };


//        [DataMember]
//        [DisplayName("Position")]
//        [Category("Input")]
//        [Description("Position of window on the screen")]
//        public Argument Position { get; set; } = new OutArgument<ScreenCoordinate>() { CanChangeType = false };

//        [DataMember]
//        [DisplayName("Dimension")]
//        [Category("Input")]
//        [Description("Dimension i.e. width and height of the window")]
//        public Argument Dimension { get; set; } = new OutArgument<Dimension>() { CanChangeType = false };

//        public GetWindowRectangleActorComponent() : base("Get Window Size", "GetWindowSize")
//        {

//        }

//        public override void Act()
//        {
//            IArgumentProcessor argumentProcessor = this.EntityManager.GetServiceOfType<IArgumentProcessor>();
//            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

//            var targetWindow = argumentProcessor.GetValue<ApplicationWindow>(this.ApplicationWindow);
//            var windowDimension = windowManager.GetWindowSize(targetWindow);

//            argumentProcessor.SetValue<Dimension>(this.Dimension, new Dimension(windowDimension.Width, windowDimension.Height));
//            argumentProcessor.SetValue<ScreenCoordinate>(this.Position, new ScreenCoordinate(windowDimension.Left, windowDimension.Top));
//        }
//    }
//}
