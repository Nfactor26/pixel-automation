using Pixel.Automation.Web.Portal.ViewModels;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Services
{
    public interface IProjectStatisticsService
    {
        Task<ProjectStatisticsViewModel> GetProjectStatisticsByProjectIdAsync(string projectId);    
    }

    public class ProjectStatisticsService : IProjectStatisticsService
    {
        private readonly HttpClient http;

        public ProjectStatisticsService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<ProjectStatisticsViewModel> GetProjectStatisticsByProjectIdAsync(string projectId)
        {
            var result = await http.GetFromJsonAsync<ProjectStatistics>($"api/ProjectStatistics/{projectId}");
            if (result != null)
            {
                return new ProjectStatisticsViewModel(result);
            }
            return null;
        }
    }
}
