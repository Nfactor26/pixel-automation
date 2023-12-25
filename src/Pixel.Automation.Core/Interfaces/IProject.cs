using Pixel.Automation.Core.Models;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces;

internal interface IProject
{
    /// <summary>
    /// Identifier of the Project
    /// </summary>
    string ProjectId { get;}

    /// <summary>
    /// Name of the project
    /// </summary>
    string Name { get; }

    /// <summary>
    /// NameSpace for generated data models. NameSpace must be unique.
    /// </summary>
    string Namespace { get; }

    List<VersionInfo> AvailableVersions { get; }

    /// <summary>
    /// Get all the versions that are active.
    /// </summary>
    IEnumerable<VersionInfo> ActiveVersions { get; }

    /// Get all the versions that are published.
    /// </summary>
    IEnumerable<VersionInfo> PublishedVersions { get; }

    /// <summary>
    /// Latest Active version of the project
    /// </summary>
    VersionInfo LatestActiveVersion { get; }

}
