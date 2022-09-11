using Dawn;

namespace Pixel.Automation.Editor.Notifications;

public class ControlUpdatedEventArgs : EventArgs
{
    public string ControlId { get; private set; }

    public ControlUpdatedEventArgs(string controlId)
    {
        Guard.Argument(controlId).NotNull().NotEmpty();
        this.ControlId = controlId;
    }
}
