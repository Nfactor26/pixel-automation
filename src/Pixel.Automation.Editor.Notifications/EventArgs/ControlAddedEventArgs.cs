using Dawn;
using Pixel.Automation.Core.Controls;

namespace Pixel.Automation.Editor.Notifications;

public class ControlAddedEventArgs : EventArgs
{
    /// <summary>
    /// Control that is added
    /// </summary>
    public ControlDescription Control { get; private set; }

    /// <summary>
    /// Identifier of test case or fixture to which prefab is added
    /// </summary>
    public string AddedTo { get; private set; }

    public ControlAddedEventArgs(ControlDescription control)
    {           
        this.Control = Guard.Argument(control).NotNull();
    }

    public ControlAddedEventArgs(ControlDescription control, string addedTo)
    {
        this.Control = Guard.Argument(control, nameof(control)).NotNull();
        this.AddedTo = Guard.Argument(addedTo, nameof(addedTo)).NotNull().NotEmpty();
    }
}
