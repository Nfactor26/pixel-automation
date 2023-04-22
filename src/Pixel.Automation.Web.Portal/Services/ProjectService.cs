using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Services
{
    public interface IProjectService
    {
        /// <summary>
        /// Get all the available session templates based on request
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AutomationProject>> GetProjectsAsync();

        /// <summary>
        /// Get AutomationProject given it's id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<AutomationProject> GetProjectByIdAsync(string projectId);

        /// <summary>
        /// Get AutomationProject given it's name
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        Task<AutomationProject> GetProjectByNameAsync(string projectName);
    }

    public class ProjectService : IProjectService
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="httpClient"></param>
        public ProjectService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AutomationProject>> GetProjectsAsync()
        {          
            return await this.httpClient.GetFromJsonAsync<IEnumerable<AutomationProject>>("api/projects");
        }

        /// <inheritdoc/>
        public async Task<AutomationProject> GetProjectByIdAsync(string projectId)
        {
            return await this.httpClient.GetFromJsonAsync<AutomationProject>($"api/projects/id/{projectId}");
        }

        /// <inheritdoc/>
        public async Task<AutomationProject> GetProjectByNameAsync(string projectName)
        {
            return await this.httpClient.GetFromJsonAsync<AutomationProject>($"api/projects/name/{projectName}");
        }
    }
}
