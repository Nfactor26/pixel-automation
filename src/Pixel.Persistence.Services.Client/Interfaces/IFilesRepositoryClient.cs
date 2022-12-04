using Pixel.Persistence.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client.Interfaces;

/// <summary>
/// Use <see cref="IFilesRepositoryClient"/> to manage the files associated with a given version of automation project
/// </summary>
public interface IProjectFilesRepositoryClient : IFilesRepositoryClient
{

}

/// <summary>
/// Use <see cref="IFilesRepositoryClient"/> to manage the files associated with a given version of prefab project
/// </summary>
public interface IPrefabFilesRepositoryClient : IFilesRepositoryClient
{

}

/// <summary>
/// Use <see cref="IFilesRepositoryClient"/> to manage the files associated with a given version of project
/// </summary>
public interface IFilesRepositoryClient
{
    /// <summary>
    /// Download  data file with specified name for a given version of project 
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<byte[]> DownProjectDataFile(string projectId, string projectVersion, string fileName);

    /// <summary>
    /// Download data files matching any of the specified tags for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    Task<byte[]> DownloadProjectDataFilesWithTags(string projectId, string projectVersion, string[] tags);

    /// <summary>
    /// Download data files with matching extension
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fileExtension"></param>
    /// <returns></returns>
    Task<byte[]> DownloadProjectDataFilesOfType(string projectId, string projectVersion, string fileExtension);

    /// <summary>
    /// Add a project data file for a given version of project
    /// </summary>
    /// <param name="file"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    Task AddProjectDataFile(ProjectDataFile file, string filePath);

    /// <summary>
    /// Delete a file by name for a given verssion of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task DeleteProjectDataFile(string projectId, string projectVersion, string fileName);
}
