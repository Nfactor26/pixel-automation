﻿using Pixel.Automation.Core;
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

/// <summary>
/// Brings the specified window to the top of the Z order. If the window is a top-level window, it is activated. 
/// If the window is a child window, the top-level parent window associated with the child window is activated. 
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get Foreground window", "Window Managment", iconSource: null, description: "Get Foreground Window", tags: new string[] { "Foreground" })]
public class GetForegroundWindowActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetForegroundWindowActorComponent>();

    [DataMember]
    [DisplayName("Foreground Window")]
    [Category("Output")]
    public Argument ForeGroundWindow { get; set; } = new OutArgument<ApplicationWindow>() { Mode = ArgumentMode.DataBound };

    public GetForegroundWindowActorComponent() : base("Get Foreground Window", "GetForegroundWindow")
    {

    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();

        var foreGroundWindow = windowManager.GetForeGroundWindow();
        logger.Information("Foreground window with title : '{0}' and hWnd : {1} was located", foreGroundWindow.WindowTitle, foreGroundWindow.HWnd);
        await argumentProcessor.SetValueAsync<ApplicationWindow>(this.ForeGroundWindow, foreGroundWindow);

        await Task.CompletedTask;
    }

}
