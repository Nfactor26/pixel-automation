using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management.Components
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
        public Argument ApplicationWindow { get; set; } = new InArgument<ApplicationWindow>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

        [DataMember]
        [DisplayName("Position")]
        [Category("Input")]
        [Description("Top left coorindates of the new position of window")]
        public Argument Position { get; set; } = new InArgument<ScreenCoordinate>() { DefaultValue = new ScreenCoordinate() };


        public SetWindowPositionActorComponent() : base("Set Window Position", "SetWindowPosition")
        {

        }

        public override async Task ActAsync()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

            var targetWindow = await argumentProcessor.GetValueAsync<ApplicationWindow>(this.ApplicationWindow);
            var newPosition = await argumentProcessor.GetValueAsync<ScreenCoordinate>(this.Position);

            windowManager.SetWindowPosition(targetWindow, newPosition);

            await Task.CompletedTask;
        }
    }
}
