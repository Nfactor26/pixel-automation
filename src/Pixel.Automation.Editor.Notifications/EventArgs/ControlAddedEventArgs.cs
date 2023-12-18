using Dawn;
using Pixel.Automation.Core.Controls;

namespace Pixel.Automation.Editor.Notifications;

/// <summary>
/// ControlAddedEventArgs captures the details of control which is added and test case or test fixture to which it was added.
/// </summary>
public class ControlAddedEventArgs : EventArgs
{
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
    /// constructor
    /// </summary>
    /// <param name="control">ControlDescription of the control that was added</param>
    public ControlAddedEventArgs(ControlDescription control)
    {           
        this.Control = Guard.Argument(control).NotNull();
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="control">ControlDescription of the control that was added</param>
    /// <param name="addedToFixture">Identifier of TestFixture to which control was added</param>
    public ControlAddedEventArgs(ControlDescription control, string addedToFixture) : this(control)
    {       
        this.AddedToFixture = Guard.Argument(addedToFixture, nameof(addedToFixture)).NotNull();       
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="control">ControlDescription of the control that was added</param>
    /// <param name="addedToFixture">Identifier of TestFixture to which control was  added</param>
    /// <param name="addedToTestCase">Identifier of TestCase to which control was added</param>
    public ControlAddedEventArgs(ControlDescription control, string addedToFixture,
        string addedToTestCase) : this(control, addedToFixture)
    {        
        this.AddedToTestCase = Guard.Argument(addedToTestCase, nameof(addedToTestCase)).NotNull();
    }
}
