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
    /// Identifier of Prefab from which control is removed
    /// </summary>
    public string RemovedFromPrefab { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="control">ControlDescription of the control that was removed</param>
    public ControlRemovedEventArgs(ControlDescription control)
    {
        this.Control = Guard.Argument(control).NotNull();
    }  

    public static ControlRemovedEventArgs ForPrefab(ControlDescription control, string prefabId)
    {
        Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
        return new ControlRemovedEventArgs(control)
        {
            RemovedFromPrefab = prefabId
        };
    }

    public static ControlRemovedEventArgs ForFixture(ControlDescription control, string fixtureId)
    {
        Guard.Argument(fixtureId, nameof(fixtureId)).NotNull().NotEmpty();
        return new ControlRemovedEventArgs(control)
        {
            RemovedFromFixture = fixtureId
        };
    }

    public static ControlRemovedEventArgs ForTest(ControlDescription control, string fixtureId, string testCaseId)
    {
        Guard.Argument(fixtureId, nameof(fixtureId)).NotNull().NotEmpty();
        Guard.Argument(testCaseId, nameof(testCaseId)).NotNull().NotEmpty();
        return new ControlRemovedEventArgs(control)
        {
            RemovedFromFixture = fixtureId,
            RemovedFromTestCase = testCaseId
        };
    }
}
