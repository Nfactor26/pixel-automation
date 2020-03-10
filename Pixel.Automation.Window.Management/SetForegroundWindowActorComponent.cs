using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Window.Management
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Set Foreground Winodw", "Window Managment", iconSource: null, description: "Set window as foreground window", tags: new string[] { "Set State", "Foreground" })]
    public class SetForegroundWindowActorComponent : ActorComponent
    {
        [DataMember]
        [DisplayName("Target Window")]
        [Category("Input")]
        public Argument ApplicationWindow { get; set; } = new InArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        public SetForegroundWindowActorComponent() : base("Set Foreground Window", "SetForegroundWindow")
        {

        }

        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

            var targetWindow = argumentProcessor.GetValue<ApplicationWindow>(this.ApplicationWindow);
            windowManager.SetForeGroundWindow(targetWindow);
        }
    }
}
