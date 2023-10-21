using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Pixel.Windows.Automation;

namespace Pixel.Automation.UIA.Components;

/// <summary>
/// Use <see cref="MoveActorComponent"/> to move a control to new position.
/// Control must support <see cref="TransformPattern"/>
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Move", "UIA", "Transform", iconSource: null, description: "Trigger Transform pattern on AutomationElement to move it", tags: new string[] { "Move", "UIA" })]
public class MoveActorComponent : UIAActorComponent
{
    private readonly ILogger logger = Log.ForContext<MoveActorComponent>();

    /// <summary>
    /// New Position co-ordinates of the control
    /// </summary>
    [DataMember]
    [Display(Name = "Position", GroupName = "Configuration", Order = 10, Description = "New position co-ordinates of the control")]
    public Argument Position { get; set; } = new InArgument<ScreenCoordinate>() { Mode = ArgumentMode.Default, DefaultValue = new ScreenCoordinate()};

   /// <summary>
   /// Default constructor
   /// </summary>
    public MoveActorComponent() : base("Move", "Move")
    {

    }

    /// <summary>
    /// Move the control e.g. window or a dialog to a new position 
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws InvalidOperationException if TransformPattern is not supported</exception>      
    public override async Task ActAsync()
    {
        var (name, control) = await GetTargetControl();
        var newPosition = await this.ArgumentProcessor.GetValueAsync<ScreenCoordinate>(this.Position);           
        control.MoveTo(newPosition.XCoordinate, newPosition.YCoordinate);
        logger.Information("Control : '{0}' was moved to position : ({1}, {2})", name, newPosition.XCoordinate, newPosition.YCoordinate);
    }

}
