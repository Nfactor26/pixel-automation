using Dawn;
using Pixel.Automation.Core.Controls;
using System;

namespace Pixel.Automation.Editor.Notifications;

public class ControlAddedEventArgs : EventArgs
{
    public ControlDescription Control { get; private set; }

    public ControlAddedEventArgs(ControlDescription control)
    {           
        this.Control = Guard.Argument(control).NotNull();
    }
}
