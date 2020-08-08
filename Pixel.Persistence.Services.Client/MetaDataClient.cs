using Dawn;
using Pixel.Automation.Core;
using Pixel.Persistence.Core.Models;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class MetaDataClient : IMetaDataClient
    {
        private readonly string baseUrl;             

        public MetaDataClient(ApplicationSettings applicationSettings)
        {
            Guard.Argument(applicationSettings).NotNull();
            this.baseUrl = $"{applicationSettings.PersistenceServiceUri}/MetaData";
        }


        public async Task<IEnumerable<ApplicationMetaData>> GetApplicationMetaData()
        {
            RestRequest restRequest = new RestRequest("application");
            var client = new RestClient(baseUrl);
            var response = await client.ExecuteGetAsync<IEnumerable<ApplicationMetaData>>(restRequest);
            return response.Data;
        }


        public async Task<IEnumerable<ProjectMetaData>> GetProjectsMetaData()
        {
            RestRequest restRequest = new RestRequest("project");
            var client = new RestClient(baseUrl);
            var response = await client.ExecuteGetAsync<IEnumerable<ProjectMetaData>>(restRequest);
            return response.Data;
        }

        public async Task<IEnumerable<ProjectMetaData>> GetProjectMetaData(string projectId)
        {
            RestRequest restRequest = new RestRequest($"project/{projectId}");
            var client = new RestClient(baseUrl);
            var response = await client.ExecuteGetAsync<IEnumerable<ProjectMetaData>>(restRequest);
            return response.Data;
        }
    }
}
