using Dawn;

namespace Pixel.Automation.Editor.Notifications;

/// <summary>
/// Notification messsage whenever a prefab is removed.
/// </summary>
public class PrefabRemovedEventArgs : EventArgs
{
    /// <summary>
    /// Identifier of the prefab that is removed
    /// </summary>
    public string PrefabId { get; private set; }

    /// <summary>
    /// Identifier of test case or fixture from which prefab is removed
    /// </summary>
    public string RemovedFrom { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="prefabId"></param>
    /// <param name="removedFrom"></param>
    public PrefabRemovedEventArgs(string prefabId, string removedFrom)
    {
        this.PrefabId = Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
        this.RemovedFrom = Guard.Argument(removedFrom, nameof(removedFrom)).NotNull().NotEmpty();
    }
}
