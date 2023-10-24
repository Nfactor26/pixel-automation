using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
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
[ToolBoxItem("Resize Window", "Window Managment", iconSource: null, description: "Resize window to new width and height", tags: new string[] { "Resize" })]
public class ResizeWindowActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<ResizeWindowActorComponent>();

    [DataMember]
    [DisplayName("Target Window")]
    [Category("Input")]
    public Argument ApplicationWindow { get; set; } = new InArgument<ApplicationWindow>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    [DataMember]
    [DisplayName("Window dimension")]
    [Category("Input")]
    [Description("New dimension i.e. width and height of window")]
    public Argument Dimension { get; set; } = new InArgument<Dimension>() { DefaultValue = new Dimension(0, 0) };

    public ResizeWindowActorComponent() : base("Resize Window", "ResizeWindow")
    {

    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

        ApplicationWindow targetWindow = await argumentProcessor.GetValueAsync<ApplicationWindow>(this.ApplicationWindow);
        Dimension windowDimension = await argumentProcessor.GetValueAsync<Dimension>(this.Dimension);
        windowManager.SetWindowSize(targetWindow, windowDimension.Width, windowDimension.Height);
        logger.Information("Window with title : '{0}' and hWnd : '{1}' was resized to width : '{3}' and height : '{4}'", targetWindow.WindowTitle,
            targetWindow.HWnd, windowDimension.Width, windowDimension.Height);

        await Task.CompletedTask;
    }
}
