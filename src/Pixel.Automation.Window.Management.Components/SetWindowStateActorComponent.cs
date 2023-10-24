using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
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
[ToolBoxItem("Set Window State", "Window Managment", iconSource: null, description: "Set state of window to Maximized/Normal/Minimized", tags: new string[] { "Set State" })]
public class SetWindowStateActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<SetWindowStateActorComponent>();

    [DataMember]
    [DisplayName("Target Window")]
    [Category("Input")]
    public Argument ApplicationWindow { get; set; } = new InArgument<ApplicationWindow>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

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
        logger.Information("State of window with title : '{0}' and hWnd : '{1}' was updated to : '{2}'", targetWindow.WindowTitle, targetWindow.HWnd, this.DesiredState);

        await Task.CompletedTask;
    }
}
