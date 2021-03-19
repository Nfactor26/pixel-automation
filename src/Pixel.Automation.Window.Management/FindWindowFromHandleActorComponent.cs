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
    [ToolBoxItem("Find Window (hWnd)", "Window Managment", iconSource: null, description: "Find a window given it's window handle", tags: new string[] { "Find", "Window" })]
    public class FindWindowFromHandleActorComponent : ActorComponent
    {
        [DataMember]
        [Description("Handle window to be located")]
        [DisplayName("Window Handle")]
        public Argument WindowHandle { get; set; } = new InArgument<IntPtr>() { DefaultValue = IntPtr.Zero, Mode = ArgumentMode.Default, CanChangeType = false, CanChangeMode = true };

        [DataMember]
        [DisplayName("Found Window")]
        [Description("Found window  matching given process id")]
        [Category("Output")]
        public Argument FoundWindow { get; set; } = new OutArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };

        public FindWindowFromHandleActorComponent() : base("Find Window From Handle", " FindWindowFromHandle")
        {

        }

        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

            IntPtr windowHandle = argumentProcessor.GetValue<IntPtr>(this.WindowHandle);
            var foundWindow = windowManager.FromHwnd(windowHandle);

            argumentProcessor.SetValue<ApplicationWindow>(this.FoundWindow, foundWindow);
        }
    }
}
