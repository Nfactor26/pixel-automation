using Dawn;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Editor.Notifications;

public class ApplicationAddedEventArgs : EventArgs
{
    public IApplicationEntity Application { get; set; }

    public ApplicationAddedEventArgs(IApplicationEntity application)
    {
        Guard.Argument(application).NotNull();
        this.Application = application;
    }
}
