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
[ToolBoxItem("Set Foreground Winodw", "Window Managment", iconSource: null, description: "Set window as foreground window", tags: new string[] { "Set State", "Foreground" })]
public class SetForegroundWindowActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<SetForegroundWindowActorComponent>();

    [DataMember]
    [DisplayName("Target Window")]
    [Category("Input")]
    public Argument ApplicationWindow { get; set; } = new InArgument<ApplicationWindow>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    public SetForegroundWindowActorComponent() : base("Set Foreground Window", "SetForegroundWindow")
    {

    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

        var targetWindow = await argumentProcessor.GetValueAsync<ApplicationWindow>(this.ApplicationWindow);
        windowManager.SetForeGroundWindow(targetWindow);
        logger.Information("Window with title : '{0}' and hWnd : '{1}' was set as the foreground window", targetWindow.WindowTitle, targetWindow.HWnd);

        await Task.CompletedTask;
    }
}
