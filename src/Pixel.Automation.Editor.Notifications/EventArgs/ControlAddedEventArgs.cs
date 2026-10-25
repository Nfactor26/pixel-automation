using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Editor.Notifications;

/// <summary>
/// ControlAddedEventArgs captures the details of control which is added and test case or test fixture to which it was added.
/// </summary>
public class ControlAddedEventArgs : EventArgs
{
    /// <summary>
    /// Target Entity to which control was added
    /// </summary>
    public Entity AddedTo { get; private set; }
    
    /// <summary>
    /// Control that was added
    /// </summary>
    public ControlDescription Control { get; private set; }

    /// <summary>
    /// Identifier of TestFixture to which control was added
    /// </summary>
    public string AddedToFixture { get; private set; }

    /// <summary>
    /// Identifier of TestCase to which control was added
    /// </summary>
    public string AddedToTestCase { get; private set; }

    /// <summary>
    /// Identifier of Prefab to which control was added
    /// </summary>
    public string AddedToPrefab { get; private set; }
       
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="control">ControlDescription of the control that was added</param>
    public ControlAddedEventArgs(Entity addedTo, ControlDescription control)
    {   
        this.AddedTo = Guard.Argument(addedTo, nameof(addedTo)).NotNull();
        this.Control = Guard.Argument(control, nameof(control)).NotNull();
    }  
    

    public static ControlAddedEventArgs ForProject(Entity addedTo, ControlDescription control)
    {
        return new ControlAddedEventArgs(addedTo, control);       
    }

    public static ControlAddedEventArgs ForPrefab(Entity addedTo, ControlDescription control, string prefabId)
    {
        Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
        return new ControlAddedEventArgs(addedTo, control)
        {
            AddedToPrefab = prefabId
        };
    }

    public static ControlAddedEventArgs ForFixture(Entity addedTo, ControlDescription control, string fixtureId)
    {
        Guard.Argument(fixtureId, nameof(fixtureId)).NotNull().NotEmpty();
        return new ControlAddedEventArgs(addedTo, control)
        {
            AddedToFixture = fixtureId
        };
    }

    public static ControlAddedEventArgs ForTest(Entity addedTo, ControlDescription control, string fixtureId, string testCaseId)
    {
        Guard.Argument(fixtureId, nameof(fixtureId)).NotNull().NotEmpty();
        Guard.Argument(testCaseId, nameof(testCaseId)).NotNull().NotEmpty();
        return new ControlAddedEventArgs(addedTo, control)
        {
            AddedToFixture = fixtureId,
            AddedToTestCase = testCaseId
        };
    }
}
