using Pixel.Automation.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client.Interfaces;

public interface IAutomationsRepositoryClient
{
    /// <summary>
    /// Get an AutomationProject by Id
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    Task<AutomationProject> GetByIdAsync(string projectId);

    /// <summary>
    /// Get an AutomationProject by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<AutomationProject> GetByNameAsync(string name);

    /// <summary>
    /// Get all the available AutomationProjects
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<AutomationProject>> GetAllAsync();

    /// <summary>
    /// Add an AutomationProject
    /// </summary>
    /// <param name="automationProject"></param>
    /// <returns></returns>
    Task<AutomationProject> AddProjectAsync(AutomationProject automationProject);

    /// <summary>
    /// Add a new version to AutomationProject by clonining data from another specified version
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="newVersion"></param>
    /// <param name="cloneFrom"></param>
    /// <returns></returns>
    Task<ProjectVersion> AddProjectVersionAsync(string projectId, ProjectVersion newVersion, ProjectVersion cloneFrom);

    /// <summary>
    /// Update version details for an AutomationProject
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="projectVersion"></param>
    /// <returns></returns>
    Task<ProjectVersion> UpdateProjectVersionAsync(string projectId, ProjectVersion projectVersion);  

}
