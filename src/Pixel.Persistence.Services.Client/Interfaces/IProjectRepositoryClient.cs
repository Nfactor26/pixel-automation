using System.Threading.Tasks;
using Pixel.Automation.Core.Models;

namespace Pixel.Persistence.Services.Client
{
    public interface IProjectRepositoryClient
    {
        /// <summary>
        /// Add or update a project (only .atm file)
        /// </summary>
        /// <param name="automationProject"></param>
        /// <param name="projectFile"></param>
        /// <returns></returns>
        Task AddOrUpdateProject(AutomationProject automationProject, string projectFile);

        /// <summary>
        /// Add or update project data files for a given version
        /// </summary>
        /// <param name="automationProject"></param>
        /// <param name="version"></param>
        /// <param name="projectFile"></param>
        /// <returns></returns>
        Task AddOrUpdateProjectDataFiles(AutomationProject automationProject, VersionInfo version, string projectFile);

        /// <summary>
        /// Get project data files for a given version
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        Task<byte[]> GetProjectDataFiles(string projectId, string version);

        /// <summary>
        /// Get project file (.atm) given it's id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<AutomationProject> GetProjectFile(string projectId);
    }
}