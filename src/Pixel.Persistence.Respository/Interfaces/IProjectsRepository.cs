using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository.Interfaces;

public interface IProjectsRepository
{
    /// <summary>
    /// Get a project by Id
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AutomationProject> FindByIdAsync(string projectId, CancellationToken cancellationToken);

    /// <summary>
    /// Get a project by name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<AutomationProject> FindByNameAsync(string name, CancellationToken cancellationToken);

    /// <summary>
    /// Get all available projects
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<AutomationProject>> FindAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Add a new project
    /// </summary>
    /// <param name="automationProject"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddProjectAsync(AutomationProject automationProject, CancellationToken cancellationToken);

    /// <summary>
    /// Add or update a version of a Project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="newVersion"></param>
    /// <param name="cloneFrom"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task AddProjectVersionAsync(string projectId, VersionInfo newVersion, VersionInfo cloneFrom, CancellationToken cancellationToken);

    /// <summary>
    /// Add or update a version of a Project
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="version"></param>    
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateProjectVersionAsync(string projectId, VersionInfo version, CancellationToken cancellationToken);

}
