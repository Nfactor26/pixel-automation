using Dawn;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class TemplateClient : ITemplateClient
    {
        private readonly ILogger logger = Log.ForContext<TemplateClient>();
        private readonly IRestClientFactory clientFactory;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="clientFactory"></param>
        public TemplateClient(IRestClientFactory clientFactory)
        {
            this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
        }

        ///<inheritdoc/>
        public async Task<SessionTemplate> GetByIdAsync(string id)
        {
            Guard.Argument(id).NotNull().NotEmpty();
            logger.Debug("Get SessionTemplate by Id : {0}", id);

            RestRequest restRequest = new RestRequest($"templates/id/{id}");
            var client = this.clientFactory.GetOrCreateClient();
            var response = await client.ExecuteGetAsync<SessionTemplate>(restRequest);
            return response.Data;
        }

        ///<inheritdoc/>
        public async Task<SessionTemplate> GetByNameAsync(string name)
        {
            Guard.Argument(name).NotNull().NotEmpty();
            logger.Debug("Get SessionTemplate by name : {0}", name);

            RestRequest restRequest = new RestRequest($"templates/name/{name}");
            var client = this.clientFactory.GetOrCreateClient();
            var response = await client.ExecuteGetAsync<SessionTemplate>(restRequest);
            return response.Data;
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<SessionTemplate>> GetAllAsync()
        {
            logger.Debug("Get all SessionTemplates");

            RestRequest restRequest = new RestRequest("templates");
            var client = this.clientFactory.GetOrCreateClient();
            var response = await client.ExecuteGetAsync<IEnumerable<SessionTemplate>>(restRequest);
            return response.Data ?? Enumerable.Empty<SessionTemplate>();
        }

        ///<inheritdoc/>
        public async Task CreateAsync(SessionTemplate sessionTemplate)
        {
            Guard.Argument(sessionTemplate).NotNull();
            logger.Debug("Add new template {@SessionTemplate}", sessionTemplate);

            RestRequest restRequest = new RestRequest("templates") { Method = Method.POST };
            restRequest.AddJsonBody(sessionTemplate);
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task UpdateAsync(SessionTemplate sessionTemplate)
        {
            Guard.Argument(sessionTemplate).NotNull();
            logger.Debug("Updated template {@SessionTemplate}", sessionTemplate);

            RestRequest restRequest = new RestRequest("templates") { Method = Method.PUT };
            restRequest.AddJsonBody(sessionTemplate);
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task DeleteAsync(string id)
        {
            Guard.Argument(id).NotNull().NotEmpty();
            logger.Debug("Delete SessionTemplate with Id : {0}", id);

            RestRequest restRequest = new RestRequest($"templates/{id}") { Method = Method.DELETE };
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task DeleteAsync(SessionTemplate sessionTemplate)
        {
            Guard.Argument(sessionTemplate).NotNull();

            await DeleteAsync(sessionTemplate.Id);
        }
    }
}
