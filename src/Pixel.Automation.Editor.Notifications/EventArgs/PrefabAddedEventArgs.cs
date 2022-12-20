using Dawn;

namespace Pixel.Automation.Editor.Notifications;

/// <summary>
/// Notification messsage whenever a prefab is added.
/// </summary>
public class PrefabAddedEventArgs : EventArgs
{
    /// <summary>
    /// Identifier of the prefab
    /// </summary>
    public string PrefabId { get; private set; }

    /// <summary>
    /// Identifier of test case or fixture to which prefab is added
    /// </summary>
    public string AddedTo { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="prefabId"></param>
    /// <param name="addedTo"></param>
    public PrefabAddedEventArgs(string prefabId, string addedTo)
    {
        this.PrefabId = Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
        this.AddedTo = Guard.Argument(addedTo, nameof(addedTo)).NotNull().NotEmpty();
    }
}
