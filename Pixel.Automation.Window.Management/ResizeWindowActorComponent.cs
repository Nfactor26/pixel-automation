using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Window.Management
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Resize Window", "Window Managment", iconSource: null, description: "Resize window to new width and height", tags: new string[] { "Resize" })]
    public class ResizeWindowActorComponent : ActorComponent
    {
        [DataMember]
        [DisplayName("Target Window")]
        [Category("Input")]
        public Argument ApplicationWindow { get; set; } = new InArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };

        [DataMember]
        [DisplayName("Window dimension")]
        [Category("Input")]
        [Description("New dimension i.e. width and height of window")]
        public Argument Dimension { get; set; } = new InArgument<Dimension>() { DefaultValue = Core.Devices.Dimension.ZeroExtents };

        public ResizeWindowActorComponent() : base("Resize Window", "ResizeWindow")
        {

        }

        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

            ApplicationWindow targetWindow = argumentProcessor.GetValue<ApplicationWindow>(this.ApplicationWindow);
            Dimension windowDimension = argumentProcessor.GetValue<Dimension>(this.Dimension);

            windowManager.SetWindowSize(targetWindow, windowDimension.Width, windowDimension.Height);
        }
    }
}
