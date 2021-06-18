using Dawn;
using Pixel.Automation.Core;
using Pixel.Persistence.Core.Models;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class TemplateClient : ITemplateClient
    {
        private readonly string baseUrl;
   
        public TemplateClient(ApplicationSettings applicationSettings)
        {
            Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            this.baseUrl = $"{applicationSettings.PersistenceServiceUri}/Templates";          
        }

        public async Task<SessionTemplate> GetByIdAsync(string Id)
        {
            RestRequest restRequest = new RestRequest($"id/{Id}");
            var client = new RestClient(baseUrl);
            var response = await client.ExecuteGetAsync<SessionTemplate>(restRequest);
            return response.Data;
        }

        public async Task<SessionTemplate> GetByNameAsync(string name)
        {
            RestRequest restRequest = new RestRequest($"name/{name}");
            var client = new RestClient(baseUrl);
            var response = await client.ExecuteGetAsync<SessionTemplate>(restRequest);
            return response.Data;
        }

        public async Task<IEnumerable<SessionTemplate>> GetAllAsync()
        {
            RestRequest restRequest = new RestRequest();
            var client = new RestClient(baseUrl);
            var response = await client.ExecuteGetAsync<IEnumerable<SessionTemplate>>(restRequest);
            return response.Data ?? Enumerable.Empty<SessionTemplate>();
        }

        public async Task CreateAsync(SessionTemplate sessionTemplate)
        {
            RestRequest restRequest = new RestRequest();
            restRequest.AddJsonBody(sessionTemplate);
            var client = new RestClient(baseUrl);
            _ = await client.PostAsync<SessionTemplate>(restRequest);           
        }

        public async Task UpdateAsync(SessionTemplate sessionTemplate)
        {
            RestRequest restRequest = new RestRequest();
            restRequest.AddJsonBody(sessionTemplate);
            var client = new RestClient(baseUrl);
            _ = await client.PutAsync<SessionTemplate>(restRequest);
        }

        public async Task DeleteAsync(string Id)
        {
            RestRequest restRequest = new RestRequest(Id);
            var client = new RestClient(baseUrl);
            _ = await client.DeleteAsync<string>(restRequest);
        }

        public async Task DeleteAsync(SessionTemplate sessionTemplate)
        {
            await DeleteAsync(sessionTemplate.Id);
        }        
    }
}
