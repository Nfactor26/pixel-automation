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
    /// <summary>
    /// Move the window to a new position
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Set Position", "Window Managment", iconSource: null, description: "Set window position", tags: new string[] { "Position" })]
    public class SetWindowPositionActorComponent : ActorComponent
    {
        [DataMember]
        [DisplayName("Target Window")]
        [Category("Input")]   
        public Argument ApplicationWindow { get; set; } = new InArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };

        [DataMember]
        [DisplayName("Position")]
        [Category("Input")]       
        [Description("Top left coorindates of the new position of window")]       
        public Argument Position { get; set; } = new InArgument<ScreenCoordinate>() { DefaultValue = new ScreenCoordinate() };
  
     
        public SetWindowPositionActorComponent() : base("Set Window Position", "SetWindowPosition")
        {

        }

        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

            var targetWindow = argumentProcessor.GetValue<ApplicationWindow>(this.ApplicationWindow);
            var newPosition = argumentProcessor.GetValue<ScreenCoordinate>(this.Position);

            windowManager.SetWindowPosition(targetWindow, newPosition);
        }

    }
}
