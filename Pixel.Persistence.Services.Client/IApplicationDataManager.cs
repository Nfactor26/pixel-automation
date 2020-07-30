using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pixel.Automation.Core.Models;

namespace Pixel.Persistence.Services.Client
{ 
    public interface IApplicationDataManager
    {
        Task AddOrUpdateApplicationAsync(ApplicationDescription applicationDescription);
        Task AddOrUpdateControlAsync(ControlDescription controlDescription);
        Task<string> AddOrUpdateControlImageAsync(ControlDescription controlDescription, Stream stream, string imageResolution);
        Task AddOrUpdateProjectAsync(AutomationProject automationProject, VersionInfo projectVersion);
        Task DownloadApplicationsDataAsync();
        Task DownloadProjectDataAsync(AutomationProject automationProject, VersionInfo projectVersion);
        Task DownloadProjectsAsync();
        IEnumerable<ApplicationDescription> GetAllApplications();
        IEnumerable<ControlDescription> GetAllControls(ApplicationDescription applicationDescription);
    }

}