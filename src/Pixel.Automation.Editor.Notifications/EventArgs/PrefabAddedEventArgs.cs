using Dawn;

namespace Pixel.Automation.Editor.Notifications;

/// <summary>
/// PrefabAddedEventArgs captures the details of prefab which was added and test case or test fixture to which it was added.
/// </summary>
public class PrefabAddedEventArgs : EventArgs
{
    /// <summary>
    /// Identifier of the prefab that was added
    /// </summary>
    public string PrefabId { get; private set; }

    /// <summary>
    /// Identifier of TestFixture to which prefab was added
    /// </summary>
    public string AddedToFixture { get; private set; }

    /// <summary>
    /// Identifier of TestCase to which prefab was added
    /// </summary>
    public string AddedToTestCase { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="prefabId">Identifier of the prefab that was added</param>
    /// <param name="addedToTestFixture">Identifier of TestFixture to which prefab was added</param>
    public PrefabAddedEventArgs(string prefabId, string addedToTestFixture)
    {
        this.PrefabId = Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
        this.AddedToFixture = Guard.Argument(addedToTestFixture, nameof(addedToTestFixture)).NotNull().NotEmpty();        
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="prefabId">Identifier of the prefab that was added</param>
    /// <param name="addedToTestFixture">Identifier of TestFixture to which prefab was added</param>
    /// <param name="addedToTestCase">Identifier of TestCase to which prefab was added</param>
    public PrefabAddedEventArgs(string prefabId, string addedToTestFixture, string addedToTestCase) : this(prefabId, addedToTestFixture)
    {       
        this.AddedToTestCase = Guard.Argument(addedToTestCase, nameof(addedToTestCase)).NotNull().NotEmpty();
    }
}
