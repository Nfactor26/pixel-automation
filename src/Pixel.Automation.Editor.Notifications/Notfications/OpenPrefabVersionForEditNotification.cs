using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Editor.Notifications.Notfications;

/// <summary>
/// Mediator notification to open a given version of prefab for edit
/// </summary>
public class OpenPrefabVersionForEditNotification
{
    /// <summary>
    /// PrefabProject to open for edit
    /// </summary>
    public PrefabProject PrefabProject { get; init; }

    /// <summary>
    /// ProjectVersion to open for edit
    /// </summary>
    public VersionInfo VersionToOpen { get; init; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="prefabProject">PrefabProject to open for edit</param>
    /// <param name="versionToOpen">PrefabVersion to open for edit</param>
    public OpenPrefabVersionForEditNotification(PrefabProject prefabProject, VersionInfo versionToOpen)
    {
        PrefabProject = prefabProject;
        VersionToOpen = versionToOpen;
    }
}
