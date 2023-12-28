using Pixel.Automation.Core.Models;
using System;

namespace Pixel.Automation.Core;

/// <summary>
/// Event Args for project loaded event 
/// </summary>
public class ProjectLoadedEventArgs : EventArgs
{
    /// <summary>
    /// Name of the project
    /// </summary>
    public string ProjectName { get; init; }

    /// <summary>
    /// Identifier of the project
    /// </summary>
    public string ProjectId { get; init; }

    /// <summary>
    /// Version of the project
    /// </summary>
    public VersionInfo ProjectVersion { get; init; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    public ProjectLoadedEventArgs(string projectName, string projectId, VersionInfo projectVersion)
    {
        this.ProjectName = projectName;
        this.ProjectId = projectId;
        this.ProjectVersion = projectVersion;
    }
}
