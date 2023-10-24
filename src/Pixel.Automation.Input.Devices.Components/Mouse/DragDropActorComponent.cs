using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices.Components;

/// <summary>
/// Use <see cref="DragDropActorComponent"/> to perform a drag action using mouse.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Drag Drop", "Input Device", "Mouse", iconSource: null, description: "Perform a drag action using mouse", tags: new string[] { "Drag" })]
public class DragDropActorComponent : InputDeviceActor
{
    private readonly ILogger logger = Log.ForContext<DragDropActorComponent>();
         
    /// <summary>
    /// Controls how the mouse moves between two points
    /// </summary>
    [DataMember]
    [Display(Name = "Smooth Mode", GroupName = "Configuration", Order = 10, Description = "Controls how the mouse moves between two points")]
    public SmoothMode SmootMode { get; set; } = SmoothMode.Interpolated;


    Target target = Target.Control;
    /// <summary>
    /// Specify whether to perform drag on a target control or arbitrary co-ordinates
    /// </summary>
    [DataMember]
    [Display(Name = "Target", GroupName = "Configuration", Order = 20, Description = "Indicates if drag source/target is a control or an arbitrary co-ordinates")]      
    [RefreshProperties(RefreshProperties.Repaint)]
    public Target Target
    {
        get
        {
            switch (this.target)
            {
                case Target.Control:
                    this.SetDispalyAttribute(nameof(SourceControl), true);
                    this.SetDispalyAttribute(nameof(TargetControl), true);
                    this.SetDispalyAttribute(nameof(DragStartPoint), false);
                    this.SetDispalyAttribute(nameof(DropEndPoint), false);
                    break;
                case Target.Point:
                    this.SetDispalyAttribute(nameof(SourceControl), false);
                    this.SetDispalyAttribute(nameof(TargetControl), false);
                    this.SetDispalyAttribute(nameof(DragStartPoint), true);
                    this.SetDispalyAttribute(nameof(DropEndPoint), true);
                    break;
            }
            return this.target;
        }
        set
        {               
            target = value;
            OnPropertyChanged();
            ValidateComponent();
        }
    }



    /// <summary>
    /// Drag source control
    /// </summary>
    [DataMember]
    [Display(Name = "Drag Source", GroupName = "Control Details", Order = 30, Description = "Control to drag")]
    public Argument SourceControl { get; set; } = new InArgument<UIControl>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Drop target control
    /// </summary>
    [DataMember]
    [Display(Name = "Drop Target", GroupName = "Control Details", Order = 40, Description = "Target control to drop to")]
    public Argument TargetControl { get; set; } = new InArgument<UIControl>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };


    /// <summary>
    /// Drag point co-ordinates
    /// </summary>
    [DataMember]
    [Display(Name = "Drag Start", GroupName = "Point", Order = 50, Description = "Represents the coordinates which acts as drag start point")]     
    public Argument DragStartPoint { get; set; } = new InArgument<ScreenCoordinate>() { DefaultValue = new ScreenCoordinate(0, 0) };

    /// <summary>
    /// Drop point co-ordinates
    /// </summary>
    [DataMember]
    [Display(Name = "Drop End", GroupName = "Point", Order = 60, Description = "Represents the coordinates which acts as drop end point")]
    public Argument DropEndPoint { get; set; } = new InArgument<ScreenCoordinate>() { DefaultValue = new ScreenCoordinate(100, 100), CanChangeType = false };

    /// <summary>
    /// Default constructor
    /// </summary>
    public DragDropActorComponent() : base("Drag Drop", "DragDrop")
    {
        
    }


    /// <summary>
    /// Initiate a drag on a source control or an arbitary co-ordinate or end a drag on a drop target control or an arbitrary co-ordinates.
    /// </summary>
    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        ScreenCoordinate dragStartPoint = default;
        ScreenCoordinate dropEndPoint = default;

        var syntheticMouse = GetMouse();
        switch (this.Target)
        {
            case Target.Control:
                var dragSourceControl = await argumentProcessor.GetValueAsync<UIControl>(this.SourceControl);
                var (dragX, dragY) = await dragSourceControl.GetClickablePointAsync();
                dragStartPoint = new ScreenCoordinate(dragX, dragY);

                var dropTargetControl = await argumentProcessor.GetValueAsync<UIControl>(this.TargetControl);
                var (dropX, dropY) = await dropTargetControl.GetClickablePointAsync();
                dropEndPoint = new ScreenCoordinate(dropX, dropY);

                break;
            case Target.Point:
                dragStartPoint = await argumentProcessor.GetValueAsync<ScreenCoordinate>(this.DragStartPoint);
                dropEndPoint = await argumentProcessor.GetValueAsync<ScreenCoordinate>(this.DropEndPoint);
                break;
        }

        //start drag
        syntheticMouse.MoveMouseTo(dragStartPoint, this.SmootMode);
        Thread.Sleep(500);
        syntheticMouse.ButtonDown(MouseButton.LeftButton);
        logger.Information("Drag started at '{0}'", dragStartPoint);

        //end drag
        syntheticMouse.MoveMouseTo(dropEndPoint, this.SmootMode);
        Thread.Sleep(500);
        syntheticMouse.ButtonUp(MouseButton.LeftButton);
        logger.Information("Drop ended at '{0}'", dropEndPoint);

    }

    public override bool ValidateComponent()
    {
        IsValid = true;
        switch (this.Target)
        {
            case Target.Point:
                IsValid = (this.DragStartPoint?.IsConfigured() ?? false) && (this.DropEndPoint?.IsConfigured() ?? false);
                break;
            case Target.Control:
                IsValid = (this.TargetControl?.IsConfigured() ?? false) || (this.SourceControl?.IsConfigured() ?? false);
                break;
        }
        return IsValid && base.ValidateComponent();
    }

}
