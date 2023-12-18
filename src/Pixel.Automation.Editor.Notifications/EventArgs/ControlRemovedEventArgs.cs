using Dawn;
using Pixel.Automation.Core.Controls;

namespace Pixel.Automation.Editor.Notifications;

/// <summary>
/// ControlRemovedEventArgs captures the details of control which is deleted and test case or test fixture from where it is deleted.
/// </summary>
public class ControlRemovedEventArgs : EventArgs
{
    /// <summary>
    /// Control that was removed
    /// </summary>
    public ControlDescription Control { get; private set; }

    /// <summary>
    /// Identifier of TestFixture from which control is removed
    /// </summary>
    public string RemovedFromFixture { get; private set; }

    /// <summary>
    /// Identifier of TestCase from which control is removed
    /// </summary>
    public string RemovedFromTestCase { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="control">ControlDescription of the control that was removed</param>
    public ControlRemovedEventArgs(ControlDescription control)
    {
        this.Control = Guard.Argument(control).NotNull();
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="control">ControlDescription of the control that was removed</param>
    /// <param name="removedFromFixture">Identifier of TestFixture from which control is removed</param>
    public ControlRemovedEventArgs(ControlDescription control, string removedFromFixture) : this(control)
    {
        this.RemovedFromFixture = Guard.Argument(removedFromFixture, nameof(removedFromFixture)).NotNull();
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="control">ControlDescription of the control that was removed</param>
    /// <param name="removedFromFixture">Identifier of TestFixture from which control is removed</param>
    /// <param name="removedFromTestCase">Identifier of TestCase from which control is removed</param>
    public ControlRemovedEventArgs(ControlDescription control, string removedFromFixture,
        string removedFromTestCase) : this(control, removedFromFixture)
    {
        this.RemovedFromTestCase = Guard.Argument(removedFromTestCase, nameof(removedFromTestCase)).NotNull();
    }
}
