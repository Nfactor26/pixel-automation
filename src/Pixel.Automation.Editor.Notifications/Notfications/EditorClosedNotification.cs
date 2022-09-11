using Dawn;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Editor.Notifications;

public class EditorClosedNotification
{
    public AutomationProject AutomationProject { get; }

    public EditorClosedNotification(AutomationProject automationProject)
    {
        this.AutomationProject = Guard.Argument(automationProject).NotNull();            
    }
}
