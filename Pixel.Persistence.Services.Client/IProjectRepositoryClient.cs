using System.Threading.Tasks;
using Pixel.Automation.Core.Models;

namespace Pixel.Persistence.Services.Client
{
    public interface IProjectRepositoryClient
    {
        Task<string> AddOrUpdateProject(AutomationProject automationProject, string projectFile);
        Task<string> AddOrUpdateProjectDataFiles(AutomationProject automationProject, VersionInfo version, string projectFile);
        Task<byte[]> GetProjectDataFiles(string projectId, string version);
        Task<AutomationProject> GetProjectFile(string projectId);
    }
}