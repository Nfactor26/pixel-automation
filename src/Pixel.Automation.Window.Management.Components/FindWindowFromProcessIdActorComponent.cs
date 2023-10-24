using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management.Components;

[DataContract]
[Serializable]
[ToolBoxItem("Find Window (PID)", "Window Managment", iconSource: null, description: "Find a window given it's process id", tags: new string[] { "Find", "Window" })]
public class FindWindowFromProcessIdActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<FindWindowFromProcessIdActorComponent>();

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
        logger.Information("Window with title : '{0}'and hWnd : '{1}' was located for process ID : '{2}'", foundWindow.WindowTitle, foundWindow.HWnd, processId);
        await argumentProcessor.SetValueAsync<ApplicationWindow>(this.FoundWindow, foundWindow);

        await Task.CompletedTask;
    }
}
