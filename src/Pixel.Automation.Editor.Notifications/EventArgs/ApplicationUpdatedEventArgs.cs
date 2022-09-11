using Dawn;

namespace Pixel.Automation.Editor.Notifications;

public class ApplicationUpdatedEventArgs : EventArgs
{
    public string ApplicationId { get; set; }

    public ApplicationUpdatedEventArgs(string applicationId)
    {
        Guard.Argument(applicationId).NotNull().NotEmpty();
        this.ApplicationId = applicationId;
    }
}
