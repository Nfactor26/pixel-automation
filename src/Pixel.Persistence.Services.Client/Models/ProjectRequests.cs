using Pixel.Automation.Core.Models;

namespace Pixel.Persistence.Services.Client.Models;

/// <summary>
/// Request data to add a new file to the specified version of project
/// </summary>
/// <param name="ProjectId"></param>
/// <param name="ProjectVersion"></param>
/// <param name="Tag"></param>
/// <param name="FileName"></param>
/// <param name="FilePath"></param>
public record AddProjectFileRequest(string ProjectId, string ProjectVersion, string Tag, string FileName, string FilePath);

/// <summary>
/// Request data to delete a file from a specified version of project
/// </summary>
/// <param name="ProjectId"></param>
/// <param name="ProjectVersion"></param>
/// <param name="FileName"></param>
public record DeleteProjectFileRequest(string ProjectId, string ProjectVersion, string FileName);

/// <summary>
/// Request data to add a new version of the project cloned from an existing specified version
/// </summary>
/// <param name="NewVersion"></param>
/// <param name="CloneFrom"></param>
public record AddProjectVersionRequest(VersionInfo NewVersion, VersionInfo CloneFrom);



