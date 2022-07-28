using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Find Window (PID)", "Window Managment", iconSource: null, description: "Find a window given it's process id", tags: new string[] { "Find", "Window" })]
    public class FindWindowFromProcessIdActorComponent : ActorComponent
    {
        [DataMember]
        [Description("ProcessId of window to be located")]
        [DisplayName("Process Id")]
        public Argument ProcessId { get; set; } = new InArgument<int>() { Mode = ArgumentMode.Default, DefaultValue = 0 };

        [DataMember]
        [DisplayName("Found Window")]
        [Description("Found window  matching given process id")]
        [Category("Output")]
        public Argument FoundWindow { get; set; } = new OutArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };

        public FindWindowFromProcessIdActorComponent() : base("Find Window From ProcessId", " FindWindowFromProcessId")
        {

        }

        public override async Task ActAsync()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

            int processId = await argumentProcessor.GetValueAsync<int>(this.ProcessId);
            var foundWindow = windowManager.FromProcessId(processId);

            await argumentProcessor.SetValueAsync<ApplicationWindow>(this.FoundWindow, foundWindow);

            await Task.CompletedTask;
        }
    }
}
