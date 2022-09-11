namespace Pixel.Automation.Editor.Notifications;

public class PropertyGridObjectEventArgs : EventArgs
{
    public object ObjectToDisplay { get; private set; }

    public bool IsReadOnly { get; private set; }

    public Action SaveCommand { get; private set; }

    public Func<bool> CanSaveCommand { get; private set; }

    public PropertyGridObjectEventArgs(Object objectToDisplay) : this(objectToDisplay, false)
    {
     
    }

    public PropertyGridObjectEventArgs(Object objectToDisplay, bool isReadOnly) : base()
    {
        this.ObjectToDisplay = objectToDisplay;
        this.IsReadOnly = isReadOnly;
    }

    public PropertyGridObjectEventArgs(Object objectToDisplay, Action saveCommand, Func<bool> canSaveCommand) : this(objectToDisplay, false)
    {
        this.SaveCommand = saveCommand;
        this.CanSaveCommand = canSaveCommand;
    }

}
