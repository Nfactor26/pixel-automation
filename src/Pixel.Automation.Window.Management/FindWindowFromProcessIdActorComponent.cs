using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Pixel.Automation.Window.Management
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Find Window (PID)", "Window Managment", iconSource: null, description: "Find a window given it's process id", tags: new string[] { "Find", "Window" })]  
    public class FindWindowFromProcessIdActorComponent : ActorComponent
    {    
        [DataMember]
        [Description("ProcessId of window to be located")]
        [DisplayName("Process Id")]
        public Argument ProcessId { get; set; } = new InArgument<int>() { DefaultValue = 0, Mode = ArgumentMode.Default, CanChangeType = false, CanChangeMode = true };

        [DataMember]
        [DisplayName("Found Window")]
        [Description("Found window  matching given process id")]
        [Category("Output")]
        public Argument FoundWindow { get; set; } = new OutArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };

        public FindWindowFromProcessIdActorComponent() : base("Find Window From ProcessId", " FindWindowFromProcessId")
        {

        }

        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

            int processId = argumentProcessor.GetValue<int>(this.ProcessId);           
            var foundWindow = windowManager.FromProcessId(processId);

            argumentProcessor.SetValue<ApplicationWindow>(this.FoundWindow, foundWindow);
        }
    }
}
