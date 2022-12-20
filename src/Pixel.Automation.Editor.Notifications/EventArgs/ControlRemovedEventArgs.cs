using Dawn;

namespace Pixel.Automation.Editor.Notifications;

public class ControlRemovedEventArgs : EventArgs
{
    /// <summary>
    /// Identifier of the control that is removed
    /// </summary>
    public string ControlId { get; private set; }

    /// <summary>
    /// Identifier of test case or fixture from which control is removed
    /// </summary>
    public string RemovedFrom { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="controlId"></param>
    /// <param name="removedFrom"></param>
    public ControlRemovedEventArgs(string controlId, string removedFrom)
    {
        this.ControlId = Guard.Argument(controlId, nameof(controlId)).NotNull().NotEmpty();
        this.RemovedFrom = Guard.Argument(removedFrom, nameof(removedFrom)).NotNull().NotEmpty();
    }
}
