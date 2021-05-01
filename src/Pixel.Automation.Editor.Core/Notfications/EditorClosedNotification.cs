using Dawn;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Editor.Core.Notfications
{
    public class EditorClosedNotification
    {
        public AutomationProject AutomationProject { get; }

        public EditorClosedNotification(AutomationProject automationProject)
        {
            this.AutomationProject = Guard.Argument(automationProject).NotNull();            
        }
    }
}
