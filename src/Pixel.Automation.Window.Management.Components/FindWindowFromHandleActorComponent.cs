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
    [ToolBoxItem("Find Window (hWnd)", "Window Managment", iconSource: null, description: "Find a window given it's window handle", tags: new string[] { "Find", "Window" })]
    public class FindWindowFromHandleActorComponent : ActorComponent
    {
        [DataMember]
        [Description("Handle window to be located")]
        [DisplayName("Window Handle")]
        public Argument WindowHandle { get; set; } = new InArgument<IntPtr>() { Mode = ArgumentMode.Default, DefaultValue = IntPtr.Zero };

        [DataMember]
        [DisplayName("Found Window")]
        [Description("Found window  matching given process id")]
        [Category("Output")]
        public Argument FoundWindow { get; set; } = new OutArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };

        public FindWindowFromHandleActorComponent() : base("Find Window From Handle", " FindWindowFromHandle")
        {

        }

        public override async Task ActAsync()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

            IntPtr windowHandle = await argumentProcessor.GetValueAsync<IntPtr>(this.WindowHandle);
            var foundWindow = windowManager.FromHwnd(windowHandle);

            await argumentProcessor.SetValueAsync<ApplicationWindow>(this.FoundWindow, foundWindow);

            await Task.CompletedTask;
        }
    }
}
