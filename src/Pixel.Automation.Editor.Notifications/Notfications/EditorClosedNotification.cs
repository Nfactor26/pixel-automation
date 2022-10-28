using Dawn;

namespace Pixel.Automation.Editor.Notifications;

public class EditorClosedNotification<T> where T: class
{
    public T Project { get; }

    public EditorClosedNotification(T project)
    {
        this.Project = Guard.Argument(project).NotNull();            
    }
}
