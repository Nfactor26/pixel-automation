using Pixel.Persistence.Core.Models;

namespace Pixel.Persistence.Core.Request;

/// <summary>
/// Request data to add a new prefab
/// </summary>
/// <param name="ScreenId"></param>
/// <param name="Project"></param>
public record AddPrefabRequest(string ScreenId, PrefabProject Project);

/// <summary>
/// Request data to add a new version of project
/// </summary>
/// <param name="NewVersion"></param>
/// <param name="CloneFrom"></param>
public record AddPrefabVersionRequest(VersionInfo NewVersion, VersionInfo CloneFrom);

