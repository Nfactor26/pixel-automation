using Dawn;

namespace Pixel.Automation.Editor.Notifications;

/// <summary>
/// PrefabRemovedEventArgs captures the details of prefab which is removed and test case or test fixture from which it was removed
/// </summary>
public class PrefabRemovedEventArgs : EventArgs
{
    /// <summary>
    /// Identifier of the prefab that was removed
    /// </summary>
    public string PrefabId { get; private set; }

    /// <summary>
    /// Identifier of TestFixture from which prefab was removed
    /// </summary>
    public string RemovedFromFixture { get; private set; }

    /// <summary>
    /// Identifier of TestCase from which prefab was removed
    /// </summary>
    public string RemovedFromTestCase { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="prefabId">Identifier of the prefab that was removed</param>
    /// <param name="removedFromFixture">Identifier of TestFixture from which prefab was removed</param>
    public PrefabRemovedEventArgs(string prefabId, string removedFromFixture)
    {
        this.PrefabId = Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
        this.RemovedFromFixture = Guard.Argument(removedFromFixture, nameof(removedFromFixture)).NotNull().NotEmpty();
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="prefabId">Identifier of the prefab that was added</param>
    /// <param name="removedFromFixture">Identifier of TestFixture from which prefab was removed</param>
    /// <param name="removedFromTestCase">Identifier of TestCase from which prefab was removed</param>
    public PrefabRemovedEventArgs(string prefabId, string removedFromFixture, string removedFromTestCase) : this(prefabId, removedFromFixture)
    {
        this.RemovedFromTestCase = Guard.Argument(removedFromTestCase, nameof(removedFromTestCase)).NotNull().NotEmpty();
    }
}
