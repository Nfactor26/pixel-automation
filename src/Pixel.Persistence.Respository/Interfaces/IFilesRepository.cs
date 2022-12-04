using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository.Interfaces;

public interface IProjectFilesRepository : IFilesRepository
{

}

public interface IPrefabFilesRepository : IFilesRepository
{

}

public interface IFilesRepository
{
    /// <summary>
    /// Get all the files belonging to a given version of project and a matching tag
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    IAsyncEnumerable<ProjectDataFile> GetFilesAsync(string projectId, string projectVersion, string[] tags);

    /// <summary>
    /// Get all the files with a matching extension
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fileExtension"></param>
    /// <returns></returns>
    IAsyncEnumerable<ProjectDataFile> GetFilesOfTypeAsync(string projectId, string projectVersion, string fileExtension);

    /// <summary>
    /// Get a file by name for a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<ProjectDataFile> GetFileAsync(string projectId, string projectVersion, string fileName); 

    /// <summary>
    /// Add a new file to a given version of project
    /// </summary>
    /// <param name="dataFile"></param>
    /// <returns></returns>
    Task AddOrUpdateFileAsync(string projectId, string projectVersion, ProjectDataFile dataFile);

    /// <summary>
    /// Delete an existing file from a given version of project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task DeleteFileAsync(string projectId, string projectVersion, string fileName);
  
}
