using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("Set Window State", "Window Managment", iconSource: null, description: "Set state of window to Maximized/Normal/Minimized", tags: new string[] { "Set State" })]
    public class SetWindowStateActorComponent : ActorComponent
    {
        [DataMember]
        [DisplayName("Target Window")]
        [Category("Input")]
        public Argument ApplicationWindow { get; set; } = new InArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound, CanChangeType = false };
        
        [DataMember]
        [DisplayName("Desired State")]
        [Category("Input")]
        public WindowState DesiredState { get; set; } = WindowState.Maximize;

    
        public SetWindowStateActorComponent() : base("Set Window State", " SetWindowState")
        {

        }

        public override async Task ActAsync()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

            var targetWindow = await argumentProcessor.GetValueAsync<ApplicationWindow>(this.ApplicationWindow);
            windowManager.SetWindowState(targetWindow, this.DesiredState, false);

            await Task.CompletedTask;
        }
     
    }
}
