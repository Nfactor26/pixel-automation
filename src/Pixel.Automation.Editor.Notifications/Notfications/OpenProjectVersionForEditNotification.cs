using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Editor.Notifications.Notfications;

/// <summary>
/// Mediator notification to open a given version of project for edit
/// </summary>
public class OpenProjectVersionForEditNotification
{
    /// <summary>
    /// AutomationProject to open for edit
    /// </summary>
    public AutomationProject AutomationProject { get; init; }

    /// <summary>
    /// ProjectVersion to open for edit
    /// </summary>
    public VersionInfo VersionToOpen { get; init; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="automationProject">AutomationProject to open for edit</param>
    /// <param name="versionToOpen">ProjectVersion to open for edit</param>
    public OpenProjectVersionForEditNotification(AutomationProject automationProject, VersionInfo versionToOpen)
    {
        AutomationProject = automationProject;
        VersionToOpen = versionToOpen;
    }
}
