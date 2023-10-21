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
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices.Components;

/// <summary>
/// Use <see cref="MouseOverActorComponent"/> to move mouse cursor to a target control or an arbitrary co-ordinates.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Mouse Over", "Input Device", "Mouse", iconSource: null, description: "Move mouse to desired coordinates", tags: new string[] { "Click" })]
public class MouseOverActorComponent : InputDeviceActor
{
    private readonly ILogger logger = Log.ForContext<MouseOverActorComponent>();

    [Display(Name = "Smooth Mode", GroupName = "Configuration", Order = 10, Description = "Controls how the mouse moves between two points")]
    public SmoothMode SmootMode { get; set; } = SmoothMode.Interpolated;

    Target target = Target.Control;
    /// <summary>
    /// Specify whether to move mouse cursor to a target control or arbitrary co-ordinates
    /// </summary>
    [DataMember]
    [Display(Name = "Target", GroupName = "Configuration", Order = 20, Description = "Indicates whether to move cursor to a control or an arbitrary co-ordinates")]
    [RefreshProperties(RefreshProperties.Repaint)]
    public Target Target
    {
        get
        {
            switch (this.target)
            {
                case Target.Control:
                    this.SetDispalyAttribute(nameof(MoveTo), false);
                    this.SetDispalyAttribute(nameof(TargetControl), true);                     
                    break;
                case Target.Point:
                    this.SetDispalyAttribute(nameof(MoveTo), true);
                    this.SetDispalyAttribute(nameof(TargetControl), false);
                    break;
            }
            return this.target;
        }
        set
        {                
            this.target = value;
            OnPropertyChanged();
            ValidateComponent();
        }
    }

    /// <summary>
    /// Target control where cursor needs to be moved to. Target control is automatically identfied if component is a child of <see cref="IControlEntity"/>.
    /// However, if control has been already looked up and stored in an argument, it can be passed as an argument instead as the target control.
    /// </summary>
    [DataMember]
    [Display(Name = "Target Control", GroupName = "Control Details", Order = 30, Description = "[Optional] Target control where cursor needs to be moved.")]
    public Argument TargetControl { get; set; } = new InArgument<UIControl>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// An arbitrary co-ordinates where the cursor position should be moved.
    /// </summary>
    [DataMember]
    [Display(Name = "Move To", GroupName = "Point", Order = 40, Description = "[Optional] Arbitrary co-ordinates where mouse cursor should be moved to.")]
    public Argument MoveTo { get; set; } = new InArgument<ScreenCoordinate>() { DefaultValue = new ScreenCoordinate(100, 100) };

    /// <summary>
    /// Default constructor
    /// </summary>
    public MouseOverActorComponent() : base("Mouse Over", "MouseOver")
    {

    }

    /// <summary>
    /// Move the mouse cursor to a target control or an arbitrary co-ordinate.
    /// </summary>
    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;

        ScreenCoordinate screenCoordinate = default;
        switch (this.Target)
        {
            case Target.Control:
                screenCoordinate = await GetScreenCoordinateFromControl(this.TargetControl as InArgument<UIControl>);
                break;
            case Target.Point:
                screenCoordinate = await argumentProcessor.GetValueAsync<ScreenCoordinate>(this.MoveTo);
                break;
        }
        var syntheticMouse = GetMouse();
        syntheticMouse.MoveMouseTo(screenCoordinate, this.SmootMode);

        logger.Information("Mouse cursor was moved to '{0}'", screenCoordinate);

    }

    /// <summary>
    /// Check if component is correctly configured
    /// </summary>
    /// <returns></returns>
    public override bool ValidateComponent()
    {
        IsValid = true;
        switch (this.Target)
        {
            case Target.Point:
                IsValid = this.MoveTo?.IsConfigured() ?? false;
                break;
            case Target.Control:
                IsValid = (this.TargetControl?.IsConfigured() ?? false) || this.ControlEntity != null;
                break;
        }
        return IsValid && base.ValidateComponent();
    }

}
