using Pixel.Automation.Core.Interfaces;
using Pixel.Persistence.Core.Models;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class MetaDataClient : IMetaDataClient
    {
        private readonly string baseUrl = "http://localhost:49574/api/metadata";
        private readonly IRestClient client;
        private readonly ISerializer serializer;

        public MetaDataClient(ISerializer serializer)
        {
            this.serializer = serializer;
            this.client = new RestClient(baseUrl);
        }


        public async Task<IEnumerable<ApplicationMetaData>> GetApplicationMetaData()
        {
            RestRequest restRequest = new RestRequest("application");
            var response = await client.ExecuteGetAsync<IEnumerable<ApplicationMetaData>>(restRequest);
            return response.Data;
        }


        public async Task<IEnumerable<ProjectMetaData>> GetProjectsMetaData()
        {
            RestRequest restRequest = new RestRequest("project");
            var response = await client.ExecuteGetAsync<IEnumerable<ProjectMetaData>>(restRequest);
            return response.Data;
        }

        public async Task<IEnumerable<ProjectMetaData>> GetProjectMetaData(string projectId)
        {
            RestRequest restRequest = new RestRequest($"project/{projectId}");
            var response = await client.ExecuteGetAsync<IEnumerable<ProjectMetaData>>(restRequest);
            return response.Data;
        }
    }
}
